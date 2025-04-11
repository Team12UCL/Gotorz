using Shared.Models;

namespace Gotorz.Client.Services
{
    public class TravelPackageService
    {
        private static List<TravelPackage> _packages = new();
        public List<TravelPackage> Packages
        {
            get => _packages;
            set => _packages = value;
        }
    }
}
