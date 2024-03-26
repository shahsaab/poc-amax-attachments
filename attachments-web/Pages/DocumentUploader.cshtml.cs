using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;

public class DocumentUploaderModel : PageModel
{
    [BindProperty]
    public string ClientId { get; set; }
    [BindProperty]
    public string AgentId { get; set; }
    [BindProperty]
    public string DatabaseName { get; set; }
    [BindProperty]
    public IFormFile File { get; set; }
    public string APIEndpoint { get; set; }
    public string NotificationMessage { get; set; }
    public string NotificationType { get; set; }


    private readonly IHttpClientFactory _clientFactory;

    public DocumentUploaderModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        APIEndpoint = "https://localhost:7109/api/s3/upload";
    }

    public async void Notification(string Message, string Type)
    {
        NotificationMessage = Message;
        NotificationType = Type;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (File == null || File.Length == 0)
        {
            Notification("Please select a file", "alert-warning");
            ModelState.AddModelError(string.Empty, "Please select a file.");
            return Page();
        }

        using (var client = _clientFactory.CreateClient())
        {
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent(ClientId), "clientId");
                formData.Add(new StringContent(AgentId), "agentId");
                formData.Add(new StringContent(DatabaseName), "databaseName");
                formData.Add(new StreamContent(File.OpenReadStream()), "file", File.FileName);

                var response = await client.PostAsync(APIEndpoint, formData);

                if (response.IsSuccessStatusCode)
                {
                    Notification($"Document uploaded successfully.", "alert-success");
                    return Page();
                }
                else
                {
                    Notification("Error uploading document.", "alert-danger");
                    ModelState.AddModelError(string.Empty, "Error uploading file.");

                    return Page();
                }
            }
        }
    }
}
