using ConnectCMS.Models.Setup;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;

namespace ConnectCMS.Repositories
{
	public class AccountRepository : ChildRepository
	{
		public AccountRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public AccountRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public void AddAccountStatusFlag( AccountStatusFlags flag )
		{
			var account = GetCurrentLoggedInAccount();

			//TODO THOW ERROR???
			if( account == null )
				return;

			account.StatusFlags = BitwiseUtils.AddFlag( account.StatusFlags, ( int )flag );

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void RemoveAccountStausFlag( AccountStatusFlags flag )
		{
			var account = GetCurrentLoggedInAccount();

			//TODO THOW ERROR???
			if( account == null )
				return;

			account.StatusFlags = BitwiseUtils.RemoveFlag( account.StatusFlags, ( int )flag );

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public bool HasAccountStatusFlag( AccountStatusFlags flag )
		{
			var account = GetCurrentLoggedInAccount();

			//TODO THOW ERROR???
			if( account == null )
				return false;

			return BitwiseUtils.HasFlag( account.StatusFlags, ( int )flag );
		}

		public MonsciergeDataModel.Account GetCurrentLoggedInAccount()
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			return user != null ? user.Account : null;
		}

		public SetupModel GetSetupModel()
		{
			//TODO BUILD OUT
			//var summaryItems = RootRepository.SalesForceRepository.GetSummaryItems();
			//var solutionSummary = new SolutionSummaryModel();

			//RootRepository.SalesForceRepository.FillBillingInfo(solutionSummary);

			//solutionSummary.HasDelivery = summaryItems.Any(x => x.Delivery);
			//solutionSummary.SummaryItems = summaryItems;

			var propertyInfo = new PropertyInfoModel();
			RootRepository.HotelRepository.FillPropertyInfo( propertyInfo );

			var account = RootRepository.SecurityRepository.GetLoggedInUser().Account;

			return new SetupModel
			{
				CurrentStep = 0, //BitwiseUtils.HasFlag(account.StatusFlags, (int)AccountStatusFlags.BillingCompleted) ? 1 : 0,
				//SolutionSummary = solutionSummary,
				PropertyInfo = propertyInfo
			};
		}

		public void UpdateBillingInfo( SolutionSummaryModel model )
		{
			var account = GetCurrentLoggedInAccount();
			if( model.BillingLocation != null )
			{
				account.BillingAddress1 = model.BillingLocation.Address1;
				account.BillingAddress2 = model.BillingLocation.Address2;
				account.BillingCity = model.BillingLocation.City;
				account.BillingISOCountryCode = model.BillingLocation.Country != null
					? model.BillingLocation.Country.ISOCountryCode
					: null;
				account.BillingPostalCode = model.BillingLocation.PostalCode;
			}
			if( model.DeliveryLocation != null )
			{
				account.DeliveryAddress1 = model.DeliveryLocation.Address1;
				account.DeliveryAddress2 = model.DeliveryLocation.Address2;
				account.DeliveryCity = model.DeliveryLocation.City;
				account.DeliveryISOCountryCode = model.DeliveryLocation.Country != null
					? model.DeliveryLocation.Country.ISOCountryCode
					: null;
				account.DeliveryPostalCode = model.DeliveryLocation.PostalCode;
			}

			account.DeliverySameAsBilling = model.DeliveryIsSame;

			//TODO Check if these properties need to be updated to sql
			//account.Phone = model.PhoneNumber;
			//model.Company;
			//model.DeliveryCompany;
			//model.DeliveryName;
			//model.EmailAddress;
			//model.FirstName;
			//model.HasDelivery;
			//model.LastName;
			//model.PaidBy;
			//model.PaymentType;
			//model.SelectedResidenceType;
			//model.TermsAgreed;
		}

		public void ClearAccountStatusFlag()
		{
			var account = GetCurrentLoggedInAccount();

			//TODO THOW ERROR???
			if( account == null )
				return;

			account.StatusFlags = null;

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}
	}
}