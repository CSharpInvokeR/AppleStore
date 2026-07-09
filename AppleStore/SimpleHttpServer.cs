using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Collections;
using AppleStore.Helpers;

namespace AppleStore
{
    public static class SimpleHttpServer
    {
        private static HttpListener _listener;
        private static Thread _serverThread;
        private static readonly string connectionString = ConfigHelper.GetConnectionString();

        public static void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(ConfigHelper.GetServerUrl());
            _listener.Start();

            _serverThread = new Thread(HandleRequests);
            _serverThread.Start();

            Console.WriteLine($"HTTP сервер запущен на {ConfigHelper.GetServerUrl()}");
        }

        public static void Stop()
        {
            _listener?.Stop();
            _listener?.Close();
        }

        private static void HandleRequests()
        {
            while (_listener.IsListening)
            {
                IAsyncResult result = _listener.BeginGetContext(ProcessRequest, _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private static void ProcessRequest(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState;
            if (!listener.IsListening) return;

            var context = listener.EndGetContext(result);
            var request = context.Request;
            var response = context.Response;

            string responseString = "";
            string path = request.Url.AbsolutePath;

            Console.WriteLine($"Запрос: {request.HttpMethod} {path}");

            if (path == "/api/auth/login" && request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = LoginUser(body);
                }
                response.ContentType = "application/json";
            }
            else if (path == "/api/auth/register" && request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = RegisterUser(body);
                }
                response.ContentType = "application/json";
            }
            else if (path == "/api/products" && request.HttpMethod == "GET")
            {
                responseString = GetProductsJson();
                response.ContentType = "application/json";
            }
            else if (path == "/api/cart" && request.HttpMethod == "GET")
            {
                string userId = request.QueryString["userId"];
                responseString = GetCartJson(userId);
                response.ContentType = "application/json";
            }
            else if (path == "/api/cart" && request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = AddToCart(body);
                }
                response.ContentType = "application/json";
            }
            else if (path == "/api/cart" && request.HttpMethod == "PUT")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = UpdateCart(body);
                }
                response.ContentType = "application/json";
            }
            else if (path == "/api/cart" && request.HttpMethod == "DELETE")
            {
                string userId = request.QueryString["userId"];
                string productId = request.QueryString["productId"];
                responseString = RemoveFromCart(userId, productId);
                response.ContentType = "application/json";
            }
            else if (path == "/api/orders" && request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = CreateOrder(body);
                }
                response.ContentType = "application/json";
            }
            else if (path == "/api/orders" && request.HttpMethod == "GET")
            {
                string userId = request.QueryString["userId"];
                responseString = GetOrdersJson(userId);
                response.ContentType = "application/json";
            }
            else if (path == "/api/users/profile" && request.HttpMethod == "PUT")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    responseString = UpdateProfile(body);
                }
                response.ContentType = "application/json";
            }
            else if (path.StartsWith("/Resources/"))
            {
                ServeImage(request, response);
                return;
            }
            else
            {
                responseString = "{\"error\": \"Not found\"}";
                response.StatusCode = 404;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private static void ServeImage(HttpListenerRequest request, HttpListenerResponse response)
        {
            string fileName = request.Url.AbsolutePath.Replace("/Resources/", "");
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            if (File.Exists(imagePath))
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                string ext = Path.GetExtension(fileName).ToLower();

                if (ext == ".png")
                    response.ContentType = "image/png";
                else if (ext == ".jpg" || ext == ".jpeg")
                    response.ContentType = "image/jpeg";
                else if (ext == ".gif")
                    response.ContentType = "image/gif";
                else if (ext == ".bmp")
                    response.ContentType = "image/bmp";
                else
                    response.ContentType = "application/octet-stream";

                response.ContentLength64 = imageData.Length;
                response.OutputStream.Write(imageData, 0, imageData.Length);
            }
            else
            {
                response.StatusCode = 404;
                byte[] error = Encoding.UTF8.GetBytes("{\"error\": \"Image not found\"}");
                response.OutputStream.Write(error, 0, error.Length);
            }
            response.OutputStream.Close();
        }

        private static string LoginUser(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

            string username = data["username"].ToString();
            string password = data["password"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT u.UserID, u.Username, u.Password, r.RoleName, 
                                        u.FirstName, u.Patronymic, u.LastName, u.Email, u.PhoneNumber
                                 FROM Users u 
                                 INNER JOIN Role r ON u.RoleID = r.RoleID 
                                 WHERE u.Username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPassword = reader.GetString(2);

                            if (PasswordHelper.VerifyPassword(password, hashedPassword))
                            {
                                var user = new
                                {
                                    id = reader.GetInt32(0),
                                    username = reader.GetString(1),
                                    role = reader.GetString(3),
                                    firstName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    patronymic = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    lastName = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    email = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                    phone = reader.IsDBNull(8) ? "" : reader.GetString(8)
                                };
                                return serializer.Serialize(user);
                            }
                            else
                            {
                                return "{\"error\": \"Invalid credentials\"}";
                            }
                        }
                    }
                }
            }

            return "{\"error\": \"Invalid credentials\"}";
        }

        private static string RegisterUser(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();

            try
            {
                var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

                string username = data["username"].ToString();
                string password = data["password"].ToString();
                string email = data["email"].ToString();
                string firstName = data.ContainsKey("firstName") ? data["firstName"].ToString() : "";
                string patronymic = data.ContainsKey("patronymic") ? data["patronymic"].ToString() : "";
                string lastName = data.ContainsKey("lastName") ? data["lastName"].ToString() : "";
                string phoneNumber = data.ContainsKey("phoneNumber") ? data["phoneNumber"].ToString() : "";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                {
                    return "{\"error\": \"Заполните обязательные поля\"}";
                }

                if (password.Length < 4)
                {
                    return "{\"error\": \"Пароль должен быть минимум 4 символа\"}";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            return "{\"error\": \"Пользователь с таким логином или email уже существует\"}";
                        }
                    }

                    string hashedPassword = PasswordHelper.HashPassword(password);

                    string insertQuery = @"INSERT INTO Users (Username, Password, Email, RoleID, FirstName, Patronymic, LastName, PhoneNumber, CreatedDate) 
                                          VALUES (@Username, @Password, @Email, 1, @FirstName, @Patronymic, @LastName, @PhoneNumber, GETDATE())";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Username", username);
                        insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        insertCmd.Parameters.AddWithValue("@Email", email);
                        insertCmd.Parameters.AddWithValue("@FirstName", string.IsNullOrEmpty(firstName) ? DBNull.Value : (object)firstName);
                        insertCmd.Parameters.AddWithValue("@Patronymic", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic);
                        insertCmd.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(lastName) ? DBNull.Value : (object)lastName);
                        insertCmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrEmpty(phoneNumber) ? DBNull.Value : (object)phoneNumber);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                return "{\"success\": true}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        private static string GetProductsJson()
        {
            var products = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT p.ProdID, p.ProductName, p.CatID, p.Price, p.StockQuantity, p.Foto, c.Warranty
                                 FROM Products p 
                                 INNER JOIN Categories c ON p.CatID = c.CatID
                                 WHERE p.StockQuantity > 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new
                        {
                            id = reader.GetInt32(0),
                            name = reader.GetString(1),
                            categoryId = reader.GetInt32(2),
                            price = reader.GetDecimal(3),
                            stock = reader.GetInt32(4),
                            photo = reader.IsDBNull(5) ? "picture.png" : reader.GetString(5),
                            warranty = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                        });
                    }
                }
            }

            return new JavaScriptSerializer().Serialize(products);
        }

        private static string GetCartJson(string userId)
        {
            var serializer = new JavaScriptSerializer();
            var cartItems = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT ci.ProdID, ci.Quantity, p.ProductName, p.Price, p.Foto, p.StockQuantity
                                 FROM CartItems ci
                                 INNER JOIN Products p ON ci.ProdID = p.ProdID
                                 WHERE ci.UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cartItems.Add(new
                            {
                                productId = reader.GetInt32(0),
                                quantity = reader.GetInt32(1),
                                productName = reader.GetString(2),
                                price = reader.GetDecimal(3),
                                photo = reader.IsDBNull(4) ? "picture.png" : reader.GetString(4),
                                stock = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }

            return serializer.Serialize(cartItems);
        }

        private static string AddToCart(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

            int userId = Convert.ToInt32(data["userId"]);
            int productId = Convert.ToInt32(data["productId"]);
            int quantity = Convert.ToInt32(data["quantity"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string checkQuery = "SELECT COUNT(*) FROM CartItems WHERE UserID = @UserID AND ProdID = @ProductID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    checkCmd.Parameters.AddWithValue("@ProductID", productId);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        string updateQuery = "UPDATE CartItems SET Quantity = Quantity + @Quantity WHERE UserID = @UserID AND ProdID = @ProductID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                            updateCmd.Parameters.AddWithValue("@ProductID", productId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO CartItems (UserID, ProdID, Quantity, AddedDate) VALUES (@UserID, @ProductID, @Quantity, GETDATE())";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@ProductID", productId);
                            insertCmd.Parameters.AddWithValue("@Quantity", quantity);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return "{\"success\": true}";
        }

        private static string UpdateCart(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

            int userId = Convert.ToInt32(data["userId"]);
            int productId = Convert.ToInt32(data["productId"]);
            int quantity = Convert.ToInt32(data["quantity"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE CartItems SET Quantity = @Quantity WHERE UserID = @UserID AND ProdID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    cmd.ExecuteNonQuery();
                }
            }

            return "{\"success\": true}";
        }

        private static string RemoveFromCart(string userId, string productId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM CartItems WHERE UserID = @UserID AND ProdID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    cmd.ExecuteNonQuery();
                }
            }
            return "{\"success\": true}";
        }

        private static string CreateOrder(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();

            try
            {
                var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

                int userId = Convert.ToInt32(data["userId"]);
                string deliveryAddress = data.ContainsKey("deliveryAddress") ? data["deliveryAddress"].ToString() : "";
                string email = data.ContainsKey("email") ? data["email"].ToString() : "";

                List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();

                if (data.ContainsKey("items"))
                {
                    var itemsObj = data["items"];
                    if (itemsObj is object[] arr)
                    {
                        foreach (var item in arr)
                        {
                            var itemDict = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(item));
                            items.Add(itemDict);
                        }
                    }
                    else if (itemsObj is ArrayList arrayList)
                    {
                        foreach (var item in arrayList)
                        {
                            var itemDict = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(item));
                            items.Add(itemDict);
                        }
                    }
                    else
                    {
                        string jsonString = serializer.Serialize(itemsObj);
                        var tempItems = serializer.Deserialize<List<object>>(jsonString);
                        foreach (var item in tempItems)
                        {
                            var itemDict = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(item));
                            items.Add(itemDict);
                        }
                    }
                }
                else if (data.ContainsKey("orderDetails"))
                {
                    var orderDetailsObj = data["orderDetails"];
                    if (orderDetailsObj is object[] arr)
                    {
                        foreach (var item in arr)
                        {
                            var itemDict = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(item));
                            items.Add(itemDict);
                        }
                    }
                    else if (orderDetailsObj is ArrayList arrayList)
                    {
                        foreach (var item in arrayList)
                        {
                            var itemDict = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(item));
                            items.Add(itemDict);
                        }
                    }
                    else
                    {
                        string jsonString = serializer.Serialize(orderDetailsObj);
                        items = serializer.Deserialize<List<Dictionary<string, object>>>(jsonString);
                    }
                }

                if (items.Count == 0)
                {
                    return "{\"error\": \"No items in order\"}";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            decimal totalAmount = 0;
                            foreach (var item in items)
                            {
                                decimal price = 0;
                                int quantity = 0;

                                if (item.ContainsKey("UnitPrice"))
                                    price = Convert.ToDecimal(item["UnitPrice"]);
                                else if (item.ContainsKey("price"))
                                    price = Convert.ToDecimal(item["price"]);

                                if (item.ContainsKey("Quantity"))
                                    quantity = Convert.ToInt32(item["Quantity"]);
                                else if (item.ContainsKey("quantity"))
                                    quantity = Convert.ToInt32(item["quantity"]);

                                totalAmount += price * quantity;
                            }

                            string orderQuery = @"INSERT INTO Orders (UserID, TotalAmount, Status, OrderDate) 
                                                 VALUES (@UserID, @TotalAmount, 'Оплачен', GETDATE());
                                                 SELECT SCOPE_IDENTITY();";
                            int orderId;
                            using (SqlCommand cmd = new SqlCommand(orderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                                orderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            List<object> orderItems = new List<object>();

                            foreach (var item in items)
                            {
                                int productId = 0;
                                int quantity = 0;
                                decimal price = 0;
                                string productName = "";
                                int warranty = 0;

                                if (item.ContainsKey("ProdID"))
                                    productId = Convert.ToInt32(item["ProdID"]);
                                else if (item.ContainsKey("productId"))
                                    productId = Convert.ToInt32(item["productId"]);

                                if (item.ContainsKey("Quantity"))
                                    quantity = Convert.ToInt32(item["Quantity"]);
                                else if (item.ContainsKey("quantity"))
                                    quantity = Convert.ToInt32(item["quantity"]);

                                if (item.ContainsKey("UnitPrice"))
                                    price = Convert.ToDecimal(item["UnitPrice"]);
                                else if (item.ContainsKey("price"))
                                    price = Convert.ToDecimal(item["price"]);

                                if (item.ContainsKey("productName"))
                                    productName = item["productName"].ToString();
                                string warrantyQuery = "SELECT c.Warranty FROM Products p INNER JOIN Categories c ON p.CatID = c.CatID WHERE p.ProdID = @ProductID";
                                using (SqlCommand warrantyCmd = new SqlCommand(warrantyQuery, conn, transaction))
                                {
                                    warrantyCmd.Parameters.AddWithValue("@ProductID", productId);
                                    var warrantyResult = warrantyCmd.ExecuteScalar();
                                    warranty = warrantyResult != DBNull.Value ? Convert.ToInt32(warrantyResult) : 0;
                                }

                                string detailQuery = @"INSERT INTO OrderDetails (OrdID, ProdID, Quantity, UnitPrice) 
                                                      VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)";
                                using (SqlCommand cmd = new SqlCommand(detailQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                                    cmd.Parameters.AddWithValue("@UnitPrice", price);
                                    cmd.ExecuteNonQuery();
                                }

                                string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProdID = @ProductID";
                                using (SqlCommand cmd = new SqlCommand(updateStockQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.ExecuteNonQuery();
                                }

                                string deleteCartQuery = "DELETE FROM CartItems WHERE UserID = @UserID AND ProdID = @ProductID";
                                using (SqlCommand cmd = new SqlCommand(deleteCartQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", userId);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.ExecuteNonQuery();
                                }

                                orderItems.Add(new { ProductName = productName, Quantity = quantity, Price = price, Warranty = warranty });
                            }

                            transaction.Commit();

                            if (!string.IsNullOrEmpty(email))
                            {
                                try
                                {
                                    string userEmail = email;
                                    if (string.IsNullOrEmpty(userEmail))
                                    {
                                        string queryEmail = "SELECT Email FROM Users WHERE UserID = @UserID";
                                        using (SqlCommand cmd = new SqlCommand(queryEmail, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@UserID", userId);
                                            var emailResult = cmd.ExecuteScalar();
                                            userEmail = emailResult?.ToString() ?? "";
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(userEmail))
                                    {
                                        EmailHelper.SendOrderReceipt(
                                            userEmail,
                                            orderId,
                                            totalAmount,
                                            DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                                            deliveryAddress,
                                            orderItems
                                        );
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка отправки email: {ex.Message}");
                                }
                            }

                            var result = new
                            {
                                id = orderId,
                                total = totalAmount,
                                status = "Оплачен",
                                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                items = new List<object>()
                            };

                            return serializer.Serialize(result);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return $"{{\"error\": \"{ex.Message}\"}}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        private static string GetOrdersJson(string userId)
        {
            var serializer = new JavaScriptSerializer();
            var orders = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT OrdID, TotalAmount, Status, OrderDate 
                                 FROM Orders WHERE UserID = @UserID ORDER BY OrderDate DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            var items = new List<object>();

                            using (SqlConnection conn2 = new SqlConnection(connectionString))
                            {
                                conn2.Open();
                                string itemsQuery = @"SELECT p.ProductName, od.Quantity, od.UnitPrice
                                                      FROM OrderDetails od
                                                      INNER JOIN Products p ON od.ProdID = p.ProdID
                                                      WHERE od.OrdID = @OrderID";
                                using (SqlCommand itemsCmd = new SqlCommand(itemsQuery, conn2))
                                {
                                    itemsCmd.Parameters.AddWithValue("@OrderID", orderId);
                                    using (SqlDataReader itemsReader = itemsCmd.ExecuteReader())
                                    {
                                        while (itemsReader.Read())
                                        {
                                            items.Add(new
                                            {
                                                productName = itemsReader.GetString(0),
                                                quantity = itemsReader.GetInt32(1),
                                                price = itemsReader.GetDecimal(2)
                                            });
                                        }
                                    }
                                }
                            }

                            orders.Add(new
                            {
                                id = orderId,
                                total = reader.GetDecimal(1),
                                status = reader.GetString(2),
                                date = reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                items = items
                            });
                        }
                    }
                }
            }

            return serializer.Serialize(orders);
        }

        private static string UpdateProfile(string jsonBody)
        {
            var serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(jsonBody);

            int userId = Convert.ToInt32(data["userId"]);
            string firstName = data.ContainsKey("firstName") ? data["firstName"].ToString() : "";
            string patronymic = data.ContainsKey("patronymic") ? data["patronymic"].ToString() : "";
            string lastName = data.ContainsKey("lastName") ? data["lastName"].ToString() : "";
            string phone = data.ContainsKey("phoneNumber") ? data["phoneNumber"].ToString() : "";
            string email = data.ContainsKey("email") ? data["email"].ToString() : "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE Users SET FirstName = @FirstName, Patronymic = @Patronymic, LastName = @LastName, 
                                 PhoneNumber = @PhoneNumber, Email = @Email WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", string.IsNullOrEmpty(firstName) ? DBNull.Value : (object)firstName);
                    cmd.Parameters.AddWithValue("@Patronymic", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic);
                    cmd.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(lastName) ? DBNull.Value : (object)lastName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }

            return "{\"success\": true}";
        }
    }
}