using System;
using AutodecisionCore.Contracts.Enums;

namespace AutodecisionCore.Contracts.ViewModels.Application
{
    public class Income
    {
        public int Id { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal? IncomeAmountGross { get; set; }
        public int? AdditionalIncomeTypeId { get; set; }
        public string? AdditionalIncomeTypeDescription { get; set; }
        public int TypeIncome { get; set; }
        public string? TypeIncomeDescription { get; set; }
        public StatusIncome Status { get; set; }
        public string? DescriptionStatus { get; set; }
        public string? Description { get; set; }
        public string? PayFrequency { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int ApplicationId { get; set; }       
    }
}
