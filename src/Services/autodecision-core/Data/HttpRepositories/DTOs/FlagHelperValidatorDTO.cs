using AutodecisionCore.Contracts.ViewModels.Helpers;

namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class FlagHelperValidatorDTO
    {
        public CustomerSkipAutoDenyDTO? CustomerSkipAutoDeny { get; set; }
        public bool EmployerAllowAutoDeny { get; set; }
        public List<ApplicationDocumentDTO> ApplicationDocuments { get; set; }
    }

    public class CustomerSkipAutoDenyDTO
    {
        public bool Active { get; set; }
        public DateTime? ActivatedAt { get; set; }

        public static CustomerSkipAutoDeny? MapCustomerSkipAutoDeny(CustomerSkipAutoDenyDTO? customerSkipAutoDeny)
        {
            if (customerSkipAutoDeny == null)
                return null;

            return new CustomerSkipAutoDeny()
            {
                Active = customerSkipAutoDeny.Active,
                ActivatedAt = customerSkipAutoDeny.ActivatedAt,
            };
        }
    }

    public class ApplicationDocumentDTO
    {
        public string DocumentType { get; set; }
        public bool Uploaded { get; set; }
        public string? ReviewStatus { get; set; }


        public static List<ApplicationDocuments> MapApplicationDocuments(List<ApplicationDocumentDTO> applicationDocuments)
        {
            var list = new List<ApplicationDocuments>();

            foreach (var item in applicationDocuments)
            {
                list.Add(new ApplicationDocuments()
                {
                    DocumentType = item.DocumentType,
                    Uploaded = item.Uploaded,
                    ReviewStatus = item.ReviewStatus
                });
            }

            return list;
        }
    }
}