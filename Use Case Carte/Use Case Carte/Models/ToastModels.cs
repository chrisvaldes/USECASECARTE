 
namespace Use_Case_Carte.Models
{
    public class ToastModel
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public ToastType Type { get; set; }

        public int Duration { get; set; } = 3000;
    }

    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }
}