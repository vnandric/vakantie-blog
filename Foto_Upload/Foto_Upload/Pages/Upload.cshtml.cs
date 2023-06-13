using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.Data;


namespace Foto_Upload.Pages
{
    public class UploadModel : PageModel
    {
        private readonly string _dbPath = "photocloud.db";
        private readonly string _uploadFolderPath;

        [BindProperty]
        public IFormFileCollection UploadedFiles { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "The Evenement field is required.")]
        public string Evenement { get; set; }

        public string ErrorMessage { get; set; }

        public UploadModel(IWebHostEnvironment webHostEnvironment)
        {
            _uploadFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
        }
        [Authorize]
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Inlog");
        }

        [Authorize]
        public IActionResult OnGet()
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                // Redirect to login page if the user is not logged in
                return RedirectToPage("/Inlog");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                // Redirect to login page if the user is not logged in
                return RedirectToPage("/Inlog");
            }

            if (UploadedFiles != null && UploadedFiles.Count > 0)
            {
                try
                {
                    using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                    {
                        connection.Open();

                        // Save uploaded files to the database or disk
                        foreach (var file in UploadedFiles)
                        {
                            // Generate a unique file name
                            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                            // Save the file to the disk
                            string filePath = Path.Combine("uploads", fileName);
                            string physicalPath = Path.Combine(_uploadFolderPath, fileName);

                            using (var fileStream = new FileStream(physicalPath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Save the file information to the database
                            var insertCommand = connection.CreateCommand();
                            insertCommand.CommandText = "INSERT INTO Files (FileName, FilePath, UploadedBy, Evenement) VALUES (@FileName, @FilePath, @UploadedBy, @Evenement)";
                            insertCommand.Parameters.AddWithValue("@FileName", file.FileName);
                            insertCommand.Parameters.AddWithValue("@FilePath", filePath);
                            insertCommand.Parameters.AddWithValue("@UploadedBy", username);
                            insertCommand.Parameters.AddWithValue("@Evenement", Evenement);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"An error occurred while uploading files: {ex.Message}";
                    return Page();
                }
            }

            return RedirectToPage("Upload");
        }
    }
}
