namespace ApplicationCore.States
{
    public class UserProfilePictureState
    {
        private string? _profilePicture;

        public string ProfilePicture
        {
            get => _profilePicture ?? string.Empty;
            set
            {
                _profilePicture = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
