// -----------------------------------------------------------------------
// <copyright file="PostBackParameters.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    using System.Runtime.Serialization;

    /// <summary>
    /// PostBack Parameters class
    /// </summary>
    [DataContract]
    public class PostBackParameters
    {
        /// <summary>
        /// Gets or sets added on
        /// </summary>
        [DataMember(Name = "Addedon")]
        public string AddedOn { get; set; }

        /// <summary>
        /// Gets or sets additional charges
        /// </summary>
        [DataMember(Name = "AdditionalCharges")]
        public string AdditionalCharges { get; set; }

        /// <summary>
        /// Gets or sets additional parameter
        /// </summary>
        [DataMember(Name = "Additional_param")]
        public string AdditionalParamameter { get; set; }

        /// <summary>
        /// Gets or sets address
        /// </summary>
        [DataMember(Name = "Address1")]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets address2
        /// </summary>
        [DataMember(Name = "Address2")]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [DataMember(Name = "Amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets amount split
        /// </summary>
        [DataMember(Name = "Amount_split")]
        public string AmountSplit { get; set; }

        /// <summary>
        /// Gets or sets bank ref number
        /// </summary>
        [DataMember(Name = "Bank_ref_num")]
        public string BankReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets bank code
        /// </summary>
        [DataMember(Name = "Bankcode")]
        public string BankCode { get; set; }

        /// <summary>
        /// Gets or sets called status
        /// </summary>
        [DataMember(Name = "CalledStatus")]
        public string CalledStatus { get; set; }

        /// <summary>
        /// Gets or sets card token
        /// </summary>
        [DataMember(Name = "CardToken")]
        public string CardToken { get; set; }

        /// <summary>
        /// Gets or sets merchant parameter
        /// </summary>
        [DataMember(Name = "Card_merchant_param")]
        public string CardMerchantParam { get; set; }

        /// <summary>
        /// Gets or sets card type
        /// </summary>
        [DataMember(Name = "CardType")]
        public string CartType { get; set; }

        /// <summary>
        /// Gets or sets card hash
        /// </summary>
        [DataMember(Name = "Cardhash")]
        public string CardHash { get; set; }

        /// <summary>
        /// Gets or sets card number
        /// </summary>
        [DataMember(Name = "Cardnum")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets city
        /// </summary>
        [DataMember(Name = "City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets country
        /// </summary>
        [DataMember(Name = "Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets created on
        /// </summary>
        [DataMember(Name = "CreatedOn")]
        public string CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets discount
        /// </summary>
        [DataMember(Name = "Discount")]
        public string Discount { get; set; }

        /// <summary>
        /// Gets or sets email
        /// </summary>
        [DataMember(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets encrypted PaymentId
        /// </summary>
        [DataMember(Name = "EncryptedPaymentId")]
        public string EncryptedPaymentId { get; set; }

        /// <summary>
        /// Gets or sets error
        /// </summary>
        [DataMember(Name = "Error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets error message
        /// </summary>
        [DataMember(Name = "Error_Message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets fetch
        /// </summary>
        [DataMember(Name = "FetchAPI")]
        public string FetchAPI { get; set; }

        /// <summary>
        /// Gets or sets Field1
        /// </summary>
        [DataMember(Name = "Field1")]
        public string Field1 { get; set; }

        /// <summary>
        /// Gets or sets Field2
        /// </summary>
        [DataMember(Name = "Field2")]
        public string Field2 { get; set; }

        /// <summary>
        /// Gets or sets Field3
        /// </summary>
        [DataMember(Name = "Field3")]
        public string Field3 { get; set; }

        /// <summary>
        /// Gets or sets Field4
        /// </summary>
        [DataMember(Name = "Field4")]
        public string Field4 { get; set; }

        /// <summary>
        /// Gets or sets Field5
        /// </summary>
        [DataMember(Name = "Field5")]
        public string Field5 { get; set; }

        /// <summary>
        /// Gets or sets Field6
        /// </summary>
        [DataMember(Name = "Field6")]
        public string Field6 { get; set; }

        /// <summary>
        /// Gets or sets Field7
        /// </summary>
        [DataMember(Name = "Field7")]
        public string Field7 { get; set; }

        /// <summary>
        /// Gets or sets Field8
        /// </summary>
        [DataMember(Name = "Field8")]
        public string Field8 { get; set; }

        /// <summary>
        /// Gets or sets Field9
        /// </summary>
        [DataMember(Name = "Field9")]
        public string Field9 { get; set; }

        /// <summary>
        /// Gets or sets first name
        /// </summary>
        [DataMember(Name = "Firstname")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets hash
        /// </summary>
        [DataMember(Name = "Hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets key
        /// </summary>
        [DataMember(Name = "Key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        [DataMember(Name = "Lastname")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets me code
        /// </summary>
        [DataMember(Name = "MeCode")]
        public string MeCode { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Mihpayid")]
        public string MihPaymentId { get; set; }

        /// <summary>
        /// Gets or sets mode
        /// </summary>
        [DataMember(Name = "Mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets name on card
        /// </summary>
        [DataMember(Name = "Name_on_card")]
        public string NameOnCard { get; set; }

        /// <summary>
        /// Gets or sets net amount debit
        /// </summary>
        [DataMember(Name = "Net_amount_debit")]
        public string NetAmountDebit { get; set; }

        /// <summary>
        /// Gets or sets offer availed
        /// </summary>
        [DataMember(Name = "Offer_availed")]
        public string OfferAvailed { get; set; }

        /// <summary>
        /// Gets or sets offer failure reason
        /// </summary>
        [DataMember(Name = "Offer_failure_reason")]
        public string OfferFailureReason { get; set; }

        /// <summary>
        /// Gets or sets offer key
        /// </summary>
        [DataMember(Name = "Offer_key")]
        public string OfferKey { get; set; }

        /// <summary>
        /// Gets or sets offer type
        /// </summary>
        [DataMember(Name = "Offer_type")]
        public string OfferType { get; set; }

        /// <summary>
        /// Gets or sets paisa me code
        /// </summary>
        [DataMember(Name = "Paisa_mecode")]
        public string PaisaMeCode { get; set; }

        /// <summary>
        /// Gets or sets paymentId
        /// </summary>
        [DataMember(Name = "PaymentId")]
        public string PaymentId { get; set; }

        /// <summary>
        /// Gets or sets payUMoneyId
        /// </summary>
        [DataMember(Name = "PayuMoneyId")]
        public string PayuMoneyId { get; set; }

        /// <summary>
        /// Gets or sets Type
        /// </summary>
        [DataMember(Name = "Pg_TYPE")]
        public string PgTYPE { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Pg_ref_no")]
        public string PgReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets phone
        /// </summary>
        [DataMember(Name = "Phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "PostBackParamId")]
        public string PostBackParamameterId { get; set; }

        /// <summary>
        /// Gets or sets post url
        /// </summary>
        [DataMember(Name = "PostUrl")]
        public string PostUrl { get; set; }

        /// <summary>
        /// Gets or sets product info
        /// </summary>
        [DataMember(Name = "Productinfo")]
        public string ProductInformation { get; set; }

        /// <summary>
        /// Gets or sets state
        /// </summary>
        [DataMember(Name = "State")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets status
        /// </summary>
        [DataMember(Name = "Status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets transaction Id
        /// </summary>
        [DataMember(Name = "Txnid")]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets
        /// </summary>
        [DataMember(Name = "Udf1")]
        public string Udf1 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf10")]
        public string Udf10 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf2")]
        public string Udf2 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf3")]
        public string Udf3 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf4")]
        public string Udf4 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf5")]
        public string Udf5 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf6")]
        public string Udf6 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf7")]
        public string Udf7 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf8")]
        public string Udf8 { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        [DataMember(Name = "Udf9")]
        public string Udf9 { get; set; }

        /// <summary>
        /// Gets or sets unmapped status
        /// </summary>
        [DataMember(Name = "Unmappedstatus")]
        public string UnmappedStatus { get; set; }

        /// <summary>
        /// Gets or sets version
        /// </summary>
        [DataMember(Name = "Version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets Zip code
        /// </summary>
        [DataMember(Name = "Zipcode")]
        public string ZipCode { get; set; }
    }
}