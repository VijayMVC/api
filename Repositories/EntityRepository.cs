#region using

using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeSFWrapper.SF;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

#endregion using

namespace ConnectCMS.Repositories
{
	public class EntityRepository : ChildRepository
	{
		public EntityRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public EntityRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp,
			ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public object UpdateEntityProperty( string entityKey, string property, object value )
		{
			var key = entityKey.Split( '_' );
			var table = key[ 0 ];
			var pk = key[ 1 ];
			var type = GetType();
			var result = type.GetMethod( "Update" + table ).Invoke( this, new[] { pk, property, value } );
			return result;
		}

		#region Events

		public EventDetailSection UpdateEventDetailSection( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.EventDetailSections.Include( section => section.EventDetail ).FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Event section you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( entity.EventDetail.FKEventGroup );

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventDetailSectionSponsorshipMap UpdateEventDetailSectionSponsorshipMap( string pk, string property,
			object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.EventDetailSectionSponsorshipMaps.Include( section => section.EventDetailSection.EventDetail )
					.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Event section you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( entity.EventDetailSection.EventDetail.FKEventGroup );

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventDetailSectionAttachmentMap UpdateEventDetailSectionAttachmentMap( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.EventDetailSectionAttachmentMaps.Include( section => section.EventDetailSection.EventDetail )
					.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Event section you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( entity.EventDetailSection.EventDetail.FKEventGroup );

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventDetailSectionImageMap UpdateEventDetailSectionImageMap( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.EventDetailSectionImageMaps.Include( section => section.EventDetailSection.EventDetail )
					.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Event section you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( entity.EventDetailSection.EventDetail.FKEventGroup );

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventDetail UpdateEventDetail( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity = ProxylessContext.EventDetails.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Event you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertDevicePermissions( entity.FKDevice );

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			switch( propertyType.PropertyType.Name )
			{
				case "DateTime":
					DateTime dt;
					if( !DateTime.TryParse( value.ToString(), out dt ) )
						throw new ArgumentException( "Property expects a DateTime value" );

					dt = dt.ToUniversalTime();

					if( property == "LocalEndDateTime" )
					{
						var isAllDay = entity.LocalStartDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
									   dt.TimeOfDay == new TimeSpan( 0, 0, 0 ) && entity.LocalStartDateTime <= dt;
						if( isAllDay )
						{
							dt = dt.AddDays( 1 );
						}
					}
					propertyType.SetValue( entity, dt, null );
					break;

				default:
					if( property == "RecurrenceRule" && entity.RecurrenceRule != value.ToString() )
					{
						var children = ProxylessContext.EventDetails.Where( x => x.FKRecurrenceParentId == entity.PKID ).ToList();
						children.ForEach( x => ProxylessContext.EventDetails.Remove( x ) );
					}
					propertyType.SetValue( entity, value, null );
					break;
			}

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventGroup UpdateEventGroup( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity = ProxylessContext.EventGroups.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The EventGroup you are trying to update does not exist" );

			switch( property )
			{
				case "EventAccessCode":
					RootRepository.SecurityRepository.AssertEventGroupPemission( entity.PKID );
					value = value == null ? null : value.ToString().ToUpper().Trim();

					if(
						ProxylessContext.EventGroups.FirstOrDefault(
							x =>
								x.FKDevice == entity.FKDevice && x.EventAccessCode != null &&
								( x.EventAccessCode.ToUpper().Trim() == ( ( string )value ) ) && x.PKID != pkid ) != null )
						throw new Exception( "EventAccessCode already exists" );
					break;

				default:
					RootRepository.SecurityRepository.AssertDevicePermissions( entity.FKDevice );
					break;
			}

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			propertyType.SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventLocation UpdateEventLocation( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity = ProxylessContext.EventLocations.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The EventGroup you are trying to update does not exist" );

			switch( property )
			{
				default:
					RootRepository.SecurityRepository.AssertDevicePermissions( entity.FKDevice );
					break;
			}

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			propertyType.SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public EventGroupManagerMap UpdateEventGroupManagerMap( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.EventGroupManagerMaps.Include( x => x.EventGroup )
					.Include( x => x.ContactUser )
					.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The EventGroupManagerMaps you are trying to update does not exist" );

			switch( property )
			{
				case "Email":
					RootRepository.SecurityRepository.AssertDevicePermissions( entity.EventGroup.FKDevice );
					break;

				default:
					RootRepository.SecurityRepository.AssertDevicePermissions( entity.EventGroup.FKDevice );
					break;
			}

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			propertyType.SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		#endregion Events

		#region Hotels

		public Hotel UpdateHotel( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.Hotels.Include( h => h.Devices ).FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Hotel you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( entity.Devices.First().PKID );

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public HotelDetail UpdateHotelDetail( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.HotelDetails.Include( h => h.Hotel.Devices ).FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Hotel you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( entity.Hotel.Devices.First().PKID );

			var ReflectionObject = entity.GetType();

			switch( property )
			{
				case "Address1":
					var loc1 = entity.Location;
					loc1.Address1 = value.ToString();
					entity.Address = loc1.Address;
					break;

				case "Address2":
					var loc2 = entity.Location;
					loc2.Address2 = value.ToString();
					entity.Address = loc2.Address;
					break;

				case "PostalCode":
					entity.Zip = value.ToString();
					break;

				case "Latitude":
					float lat;
					if( float.TryParse( value.ToString(), out lat ) )
						entity.Latitude = lat;
					break;

				case "Longitude":
					float lon;
					if( float.TryParse( value.ToString(), out lon ) )
						entity.Longitude = lon;
					break;

				case "FKState":
					int fkState;
					if( int.TryParse( value.ToString(), out fkState ) )
					{
						entity.State = UtilityRepository.GetAllStates().First( s => s.PKID == fkState ).ISOStateCode;
					}
					break;

				case "FKCountry":
					int fkCountry;
					if( int.TryParse( value.ToString(), out fkCountry ) )
					{
						entity.ISOCountryCode = UtilityRepository.GetAllCountries().First( c => c.PKID == ( int )value ).ISOCountryCode;
					}
					break;

				case "State":
					if( value == null )
					{
						entity.State = null;
					}
					else
					{
						var state = value as State;
						if( state != null )
						{
							entity.State = state.ISOStateCode;
						}
					}
					break;

				case "Country":
					if( value == null )
					{
						entity.ISOCountryCode = null;
					}
					else
					{
						var country = value as Country;
						if( country != null )
						{
							entity.ISOCountryCode = country.ISOCountryCode;
						}
					}
					break;

				default:
					ReflectionObject.GetProperty( property ).SetValue( entity, value, null );
					break;
			}

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public HotelSlug UpdateHotelSlug( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.HotelSlugs.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Hotel you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public HotelBrand UpdateHotelBrand( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.HotelBrands.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Hotel you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		public HotelBrandSlug UpdateHotelBrandSlug( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.HotelBrandSlugs.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Hotel you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		#endregion Hotels

		#region MobileApps

		public MobileApp UpdateMobileApp( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity =
				ProxylessContext.MobileApps.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The Mobile App you are trying to update does not exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			var ReflectionObject = entity.GetType();
			ReflectionObject.GetProperty( property ).SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		#endregion MobileApps

		#region SubDevice

		public SubDevice UpdateSubDevice( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity = ProxylessContext.SubDevices.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The SubDevice you are trying to update does not exist" );

			switch( property )
			{
				case "Name":
				case "FKReaderboardBackgroundImage":
					RootRepository.SecurityRepository.AssertDevicePermissions( entity.FKDevice );
					break;

				default:
					RootRepository.SecurityRepository.AssertSuperAdmin();
					break;
			}

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			propertyType.SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return entity;
		}

		#endregion SubDevice

		#region AdBoard

		public AdBoard UpdateAdBoard( string pk, string property, object value )
		{
			var pkid = int.Parse( pk );

			var entity = ProxylessContext.AdBoards.FirstOrDefault( x => x.PKID == pkid );
			if( entity == null )
				throw new InvalidDataException( "The SubDevice you are trying to update does not exist" );

			switch( property )
			{
				default:
					RootRepository.SecurityRepository.AssertAdBoardTagPermission( pkid );
					break;
			}

			var ReflectionObject = entity.GetType();
			var propertyType = ReflectionObject.GetProperty( property );
			propertyType.SetValue( entity, value, null );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			var callBackUrl = string.Format( "{0}://{1}/adboard/NotifyAdBoardChanged?id={2}", HttpContext.Current.Request.Url.Scheme,
							HttpContext.Current.Request.Url.Host, pk );

			using( var wc = new System.Net.WebClient() )
			{
				wc.Headers.Add( System.Net.HttpRequestHeader.ContentType, "application/json; charset=utf-8" );
				wc.Encoding = Encoding.UTF8;
				wc.UploadString( callBackUrl, "POST", "" );
			}

			return entity;
		}

		#endregion AdBoard
	}
}