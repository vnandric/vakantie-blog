using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Foto_Upload.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public User User { get; set; }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                string connectionString = "Data Source=photocloud.db";

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO Users (username, password) VALUES (@Username, @Password)";

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", User.Username);
                        command.Parameters.AddWithValue("@Password", User.Password);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return RedirectToPage("Inlog");
            }

            return Page();
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
