using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Utilities;
using PostSharp.Extensibility;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	[Authorize]
	public class MarketingCampaignController : ControllerBase
	{
		// GET: MarketingCampaign
		public ActionResult Index()
		{
			return RedirectToAction( "Properties" );
		}

		public ActionResult Properties()
		{
			ViewBag.Page = "Properties";
			return View();
		}

		public ActionResult Campaigns()
		{
			ViewBag.Page = "Campaigns";
			return View();
		}

		public ActionResult Screens()
		{
			ViewBag.Page = "Screens";
			return View();
		}

		public ActionResult Campaign( int? campaignId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( campaignId.HasValue )
			{
				var campaign = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaign( user.PKID, campaignId.Value ).Select( mc => new
				{
					mc.PKID,
					mc.Name,
					mc.Description,
					mc.FKDefaultScreen,
					mc.Orientation,
					DefaultScreen = new { mc.DefaultScreen.PKID, mc.DefaultScreen.Name, mc.DefaultScreen.Layout },
					mc.FKWelcomeMessageBackgroundImage,
					WelcomeMessageBackgroundImage = new
					{
						PKID = mc.WelcomeMessageBackgroundImage == null ? 0 : mc.WelcomeMessageBackgroundImage.PKID,
						Name = mc.WelcomeMessageBackgroundImage == null ? null : mc.WelcomeMessageBackgroundImage.Name,
						Path = mc.WelcomeMessageBackgroundImage == null ? null : mc.WelcomeMessageBackgroundImage.Path,
						Width = mc.WelcomeMessageBackgroundImage == null ? 0 : mc.WelcomeMessageBackgroundImage.Width,
						Height = mc.WelcomeMessageBackgroundImage == null ? 0 : mc.WelcomeMessageBackgroundImage.Height,
						FKAccount = mc.WelcomeMessageBackgroundImage == null ? 0 : mc.WelcomeMessageBackgroundImage.FKAccount
					},
					mc.WelcomeMessage,
					Exceptions =
						mc.MarketingCampaignExceptions.Select(
							e =>
								new
								{
									e.PKID,
									e.FKMarketingCampaign,
									e.FKMarketingCampaignScreen,
									e.Start,
									e.End,
									MarketingCampaign = new { e.MarketingCampaign.PKID, e.MarketingCampaign.Name },
									MarketingCampaignScreen =
										new { e.MarketingCampaignScreen.PKID, e.MarketingCampaignScreen.Name, e.MarketingCampaignScreen.Layout, e.MarketingCampaignScreen.Orientation }
								} )
				} ).FirstOrDefault();

				if( campaign != null )
				{
					ViewBag.Campaign = campaign.ToJSON();
				}
			}
			var screens = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaignScreens( user.PKID ).Select( s => new
			{
				s.PKID,
				s.Name,
				s.Layout,
				s.Orientation,
				Usage = s.MarketingCampaigns.Count()
			} );
			ViewBag.Screens = screens.ToJSON();

			return View();
		}

		public ActionResult Screen( int? screenId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( screenId.HasValue )
			{
				var campaign = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaignScreen( user.PKID, screenId.Value ).Select( mc => new
				{
					mc.PKID,
					mc.Name,
					mc.Orientation,
					mc.Description,
					mc.Layout,
					MarketingCampaignScreenSections = mc.MarketingCampaignScreenSections.OrderBy( x => x.Ordinal ).Select( ss => new
					{
						ss.PKID,
						ss.Ordinal,
						ss.FKMarketingCampaignScreen,
						ss.TransitionDuration,
						ImageMaps = ss.MarketingCampaignScreenSectionImageMaps.OrderBy( x => x.Ordinal ).Select( im => new
						{
							im.PKID,
							im.FKImage,
							im.FKMarketingCampaignScreenSection,
							im.Ordinal,
							Image = new
							{
								im.Image.PKID,
								im.Image.Path,
								im.Image.Width,
								im.Image.Height,
								im.Image.FKAccount,
								im.Image.Name
							}
						} )
					} )
				} ).FirstOrDefault();

				if( campaign != null )
				{
					ViewBag.Campaign = campaign.ToJSON();
				}
			}
			return View();
		}

		[HttpPost]
		public JsonNetResult LoadProperties()
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			var heartBeat = DateTime.UtcNow.AddMinutes( -30 );
			var hotels = ConnectCmsRepository.WelcomeTabletRepository.GetAdBoardHotels( user.PKID )
				.Select(
					h =>
						new
						{
							h.PKID,
							h.Name,
							HotelBrandName =
								h.HotelBrandMaps.Any()
									? ( h.HotelBrandMaps.Any( x => x.IsPrimary )
										? h.HotelBrandMaps.FirstOrDefault( x => x.IsPrimary ).HotelBrand.Name
										: h.HotelBrandMaps.FirstOrDefault().HotelBrand.Name )
									: "",
							PermissionTags = h.PermissionHotelTagMaps.Select( y => new { y.PermissionTag.PKID, y.PermissionTag.Name } ),
							h.HotelDetail.City,
							h.HotelDetail.State,
							h.HotelDetail.ISOCountryCode,
							AdBoards = h.AdBoards.Select(
								a =>
									new
									{
										a.PKID,
										a.Name,
										a.Orientation,
										a.FKMarketingCampaign,
										a.MarketingCampaign,
										Status = a.SubDevices.All( sd => sd.LastSeen > heartBeat ) ? "Online" : ( a.SubDevices.Any( sd => sd.LastSeen > heartBeat ) ? "Some Offline" : "Offline" )
									} )
						} ).ToList();
			var marketingCampaigns = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaignLookups( user.PKID )
				.Select(
					x =>
						new
						{
							x.PKID,
							x.Name,
							x.Orientation,
							x.FKDefaultScreen,
							DefaultScreen =
								new
								{
									x.DefaultScreen.PKID,
									x.DefaultScreen.Name,
									x.DefaultScreen.Description,
									x.DefaultScreen.Orientation,
									PermissionTags =
										x.DefaultScreen.MarketingCampaignScreenPermissionTagMaps.Select(
											y => new { y.PermissionTag.PKID, y.PermissionTag.Name } )
								},
							PermissionTags =
								x.MarketingCampaignPermissionsTagMaps.Select( y => new { y.PermissionTag.PKID, y.PermissionTag.Name } )
						} ).ToList();
			return JsonNet( new { Hotels = hotels, MarketingCampaigns = marketingCampaigns } );
		}

		[HttpPost]
		public JsonNetResult LoadMarketingCampaigns()
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			var marketingCampaigns = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaigns( user.PKID )
				.Select(
					x =>
						new
						{
							x.PKID,
							x.Name,
							x.Orientation,
							x.FKDefaultScreen,
							DefaultScreen =
								new
								{
									x.DefaultScreen.PKID,
									x.DefaultScreen.Name,
									x.DefaultScreen.Orientation,
									x.DefaultScreen.Description,
									PermissionTags = x.DefaultScreen.MarketingCampaignScreenPermissionTagMaps.Select( y => new { y.PermissionTag.PKID, y.PermissionTag.Name } )
								},
							PermissionTags = x.MarketingCampaignPermissionsTagMaps.Select( y => new { y.PermissionTag.PKID, y.PermissionTag.Name } ),
							Usage = x.AdBoards.Count(),
							Exceptions = x.MarketingCampaignExceptions.Select( e => new
							{
								e.PKID,
								e.FKMarketingCampaign,
								e.FKMarketingCampaignScreen,
								e.Start,
								e.End
							} )
						} ).ToList();
			return JsonNet( new { MarketingCampaigns = marketingCampaigns } );
		}

		[HttpPost]
		public JsonNetResult LoadScreens()
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			var screens = ConnectCmsRepository.MarketingCampaignRepository.GetMarketingCampaignScreens( user.PKID )
				.Select(
					x =>
						new
						{
							x.PKID,
							x.Name,
							x.Orientation,
							x.Layout,
							PermissionTags = x.MarketingCampaignScreenPermissionTagMaps.Select( y => new { y.PermissionTag.PKID, y.PermissionTag.Name } ),
							Usage = x.MarketingCampaigns.Count()
						} ).ToList();
			return JsonNet( new { Screens = screens } );
		}

		[HttpPost]
		public JsonNetResult DeleteCampaign( int id )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.MarketingCampaignRepository.DeleteCampaign( user.PKID, id );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult DeleteScreen( int id )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.MarketingCampaignRepository.DeleteScreen( user.PKID, id );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult DeleteScreenException( int id )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.MarketingCampaignRepository.DeleteScreenException( user.PKID, id );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult SaveCampaign( MarketingCampaign campaign )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.MarketingCampaignRepository.SaveMarketingCampaign( user.PKID, campaign );
			var adboardIds =
				ConnectCmsRepository.WelcomeTabletRepository.GetAdBoardsUsingCampaign( campaign.PKID ).Select( x => x.PKID );
			Task.Run( () =>
			{
				foreach( var adboard in adboardIds )
				{
					try
					{
						var callBackUrl = string.Format( "{0}://{1}/adboard/NotifyAdBoardChanged?id={2}", Request.Url.Scheme,
							Request.Url.Host, adboard );
						using( var wc = new System.Net.WebClient() )
						{
							wc.Headers.Add( System.Net.HttpRequestHeader.ContentType, "application/json; charset=utf-8" );
							wc.Encoding = Encoding.UTF8;
							wc.UploadString( callBackUrl, "POST", "" );
						}
					}
					catch( Exception ex )
					{
						Logger.LogException( ex );
					}
				}
			} );

			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult SaveScreen( MarketingCampaignScreen screen )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.MarketingCampaignRepository.SaveMarketingCampaignScreen( user.PKID, screen );
			var adboardIds =
				ConnectCmsRepository.WelcomeTabletRepository.GetAdBoardsUsingCampaignScreen( screen.PKID ).Select( x => x.PKID );
			Task.Run( () =>
			{
				foreach( var adboard in adboardIds )
				{
					try
					{
						var callBackUrl = string.Format( "{0}://{1}/adboard/NotifyAdBoardChanged?id={2}", Request.Url.Scheme,
							Request.Url.Host, adboard );
						using( var wc = new System.Net.WebClient() )
						{
							wc.Headers.Add( System.Net.HttpRequestHeader.ContentType, "application/json; charset=utf-8" );
							wc.Encoding = Encoding.UTF8;
							wc.UploadString( callBackUrl, "POST", "" );
						}
					}
					catch( Exception ex )
					{
						Logger.LogException( ex );
					}
				}
			} );
			return JsonNet( true );
		}
	}
}