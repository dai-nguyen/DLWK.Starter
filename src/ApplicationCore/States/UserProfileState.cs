namespace ApplicationCore.States
{
    public class UserProfileState
    {
        private string? _profilePicture;
        private string? _fullName { get; set; }
        private string? _title { get; set; }

        public string ProfilePicture
        {
            get => _profilePicture ?? string.Empty;
            set
            {
                _profilePicture = value;
                NotifyStateChanged();
            }
        }

        public string FullName 
        {
            get => _fullName ?? string.Empty;
            set
            {
                _fullName = value ?? string.Empty;
                NotifyStateChanged();
            }
        }

        public string Title
        {
            get => _title ?? string.Empty;
            set
            {
                _title = value ?? string.Empty;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
