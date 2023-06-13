using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace vakantie_blog.Pages
{
    public class FotoGridModel : PageModel
    {
        private readonly string _dbPath = "photocloud.db";
        private readonly string _uploadFolderPath;

        public List<PhotoModel> Photos { get; set; }

        public FotoGridModel(IWebHostEnvironment webHostEnvironment)
        {
            _uploadFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
        }

        public IActionResult OnGet()
        {
            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT FileName, FilePath, UploadedBy, Locatie FROM Files";

                var photos = new List<PhotoModel>();

                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var photo = new PhotoModel
                        {
                            FileName = reader.GetString(0),
                            FilePath = reader.GetString(1),
                            UploadedBy = reader.GetString(2),
                            Locatie = reader.GetString(3)
                        };

                        photos.Add(photo);
                    }
                }

                Photos = photos;
            }

            return Page();
        }

        public IActionResult OnPostDelete(string filePath)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Inlog");
            }

            var uploadedByCurrentUser = Photos.FirstOrDefault(p => p.FilePath == filePath && p.UploadedBy == User.Identity.Name);
            if (uploadedByCurrentUser == null)
            {
                return NotFound();
            }

            // Delete the file from the uploads folder
            var fullPath = Path.Combine(_uploadFolderPath, filePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            // Delete the record from the database
            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Files WHERE FilePath = @filePath";
                deleteCommand.Parameters.AddWithValue("@filePath", filePath);
                deleteCommand.ExecuteNonQuery();
            }

            // Refresh the list of photos
            OnGet();

            return Page();
        }
    }
    public class PhotoModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public string Locatie { get; set; }
        public string FullImagePath => Path.Combine("/uploads", Path.GetFileName(FilePath));
    }
}
