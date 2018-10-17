// -----------------------------------------------------------------------
// <copyright file="CustomerViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The customer view model.
    /// </summary>
    public class CustomerViewModel
    {
        /// <summary>
        /// Gets or sets the microsoft Id.
        /// </summary>
        public string MicrosoftId { get; set; }

        /// <summary>
        /// Gets or sets the customer's country.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileCompanyCountryRequired")]
        [Display(Name = "CustomerProfileCompanyCountryCaption", ResourceType = typeof(Resources))]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the customer's company name.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerOrganizationRequired")]
        [Display(Name = "CustomerOrganizationNameCaption", ResourceType = typeof(Resources))]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the customer's first address line.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileAddressLine1Required")]
        [Display(Name = "CustomerProfileAddressLine1Caption", ResourceType = typeof(Resources))]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the customer's second address line.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the customer's city.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileCityRequired")]
        [Display(Name = "CustomerProfileCityCaption", ResourceType = typeof(Resources))]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the customer's state.
        /// </summary>        
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the customer's zip code.
        /// </summary>
        [Required]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the customer's language. 
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the customer's email.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileEmailAddressIdRequired")]
        [Display(Name = "CustomerProfileEmailAddressIdCaption", ResourceType = typeof(Resources))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileEmailAddressIdRequired")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileFirstNameRequired")]
        [Display(Name = "CustomerProfileFirstNameCaption", ResourceType = typeof(Resources))]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileLastNameRequired")]
        [Display(Name = "CustomerProfileLastNameCaption", ResourceType = typeof(Resources))]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PhoneHeaderRequired")]
        [Display(Name = "PhoneHeaderCaption", ResourceType = typeof(Resources))]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's domain prefix.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CustomerProfileDomainPrefixRequired")]
        [Display(Name = "CustomerProfileDomainPrefixCaption", ResourceType = typeof(Resources))]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "DomainPrefixValidationMessage")]
        public string DomainPrefix { get; set; }

        /// <summary>
        /// Gets or sets the admin user account.
        /// </summary>
        public string AdminUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the customers billing culture
        /// </summary>
        public string BillingCulture { get; set; }

        /// <summary>
        /// Gets or sets the customers billing language;
        /// </summary>
        public string BillingLanguage { get; set; }

        /// <summary>
        /// Gets or sets the Domain name
        /// </summary>
        public string DomainName { get; set; }
    }
}