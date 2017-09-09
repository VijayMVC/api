using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class MarketingCampaignRepository : ChildRepository
	{
		public MarketingCampaignRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public MarketingCampaignRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<MarketingCampaign> GetMarketingCampaign( int id )
		{
			return Rp.ExecuteAction(
				() =>
					ProxylessContext.MarketingCampaigns.Where(
						x => x.PKID == id )
				);
		}

		public IQueryable<MarketingCampaign> GetMarketingCampaign( int userId, int id )
		{
			return Rp.ExecuteAction(
					() =>
						ProxylessContext.MarketingCampaigns.Where(
							x => x.PKID == id && ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
								urm =>
									urm.UserRolePermissionTagMaps.All(
										urptm => x.MarketingCampaignPermissionsTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
										)
								)
							)
					);
		}

		public IQueryable<MarketingCampaign> GetMarketingCampaigns( int userId )
		{
			return Rp.ExecuteAction( () => ProxylessContext.MarketingCampaigns
				.Include( x => x.MarketingCampaignPermissionsTagMaps.Select( y => y.PermissionTag ) )
				.Include( x => x.DefaultScreen.MarketingCampaignScreenPermissionTagMaps.Select( y => y.PermissionTag ) )
				.Where(
					x =>
						ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
							urm =>
								urm.UserRolePermissionTagMaps.All(
									urptm => x.MarketingCampaignPermissionsTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
									)
							)
				)
				);
		}

		public IQueryable<MarketingCampaign> GetMarketingCampaignLookups( int userId )
		{
			return Rp.ExecuteAction( () => ProxylessContext.MarketingCampaigns
				.Include( x => x.MarketingCampaignPermissionsTagMaps.Select( y => y.PermissionTag ) )
				.Include( x => x.DefaultScreen.MarketingCampaignScreenPermissionTagMaps.Select( y => y.PermissionTag ) )
				.Where(
					x =>
						ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
							urm =>
								x.MarketingCampaignPermissionsTagMaps.All(
									mcptm => urm.UserRolePermissionTagMaps.Any( urptm => urptm.FKPermissionTag == mcptm.FKPermissionTag )
									)
							)
							||
						ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
							urm =>
								urm.UserRolePermissionTagMaps.All(
									urptm => x.MarketingCampaignPermissionsTagMaps.Any( mcptm => urptm.FKPermissionTag == mcptm.FKPermissionTag )
									)
							)
				)
				);
		}

		public IQueryable<MarketingCampaignScreen> GetMarketingCampaignScreens( int userId )
		{
			return Rp.ExecuteAction( () => ProxylessContext.MarketingCampaignScreens
				.Include( x => x.MarketingCampaignScreenPermissionTagMaps.Select( y => y.PermissionTag ) )
				.Where(
					x =>
						ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
							urm =>
								urm.UserRolePermissionTagMaps.All(
									urptm => x.MarketingCampaignScreenPermissionTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
									)
							)
				)
				);
		}

		public IQueryable<MarketingCampaignScreen> GetMarketingCampaignScreen( int userId, int id )
		{
			return Rp.ExecuteAction(
					() =>
						ProxylessContext.MarketingCampaignScreens.Where(
							x => x.PKID == id && ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
								urm =>
									urm.UserRolePermissionTagMaps.All(
										urptm => x.MarketingCampaignScreenPermissionTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
										)
								)
							)
					);
		}

		public IQueryable<MarketingCampaignException> GetMarketingCampaignScreenException( int userId, int id )
		{
			return Rp.ExecuteAction(
					() =>
						ProxylessContext.MarketingCampaignExceptions.Where(
							x => x.PKID == id && ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
								urm =>
									urm.UserRolePermissionTagMaps.All(
										urptm => x.MarketingCampaign.MarketingCampaignPermissionsTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
										)
								)
							)
					);
		}

		public void DeleteCampaign( int userId, int id )
		{
			var campaign = GetMarketingCampaign( userId, id ).Include( x => x.AdBoards ).FirstOrDefault();
			if( campaign != null )
			{
				ProxylessContext.MarketingCampaigns.Remove( campaign );
				ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			}
		}

		public void DeleteScreen( int userId, int id )
		{
			var screen = GetMarketingCampaignScreen( userId, id ).Include( x => x.MarketingCampaigns ).FirstOrDefault();
			if( screen != null )
			{
				ProxylessContext.MarketingCampaignScreens.Remove( screen );
				ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			}
		}

		public void DeleteScreenException( int userId, int id )
		{
			var exception = GetMarketingCampaignScreenException( userId, id ).FirstOrDefault();
			if( exception != null )
			{
				ProxylessContext.MarketingCampaignExceptions.Remove( exception );
				ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			}
		}

		public void SaveMarketingCampaign( int userId, MarketingCampaign campaign )
		{
			if( campaign.PKID != 0 )
			{
				var c1 = GetMarketingCampaign( userId, campaign.PKID ).Include( x => x.MarketingCampaignExceptions ).FirstOrDefault();
				if( c1 == null )
					return;

				c1.Name = campaign.Name;
				c1.FKDefaultScreen = campaign.FKDefaultScreen;
				c1.Orientation = campaign.Orientation;
				c1.FKWelcomeMessageBackgroundImage = campaign.FKWelcomeMessageBackgroundImage;
				c1.WelcomeMessage = campaign.WelcomeMessage;
				c1.MarketingCampaignExceptions.ToList().ForEach( e =>
				{
					var exc = campaign.MarketingCampaignExceptions.FirstOrDefault( x => x.PKID == e.PKID );
					if( exc != null )
					{
						e.FKMarketingCampaignScreen = exc.FKMarketingCampaignScreen;
						e.Start = exc.Start;
						e.End = exc.End;
					}
					else
					{
						ProxylessContext.MarketingCampaignExceptions.Remove( e );
					}
				} );
				campaign.MarketingCampaignExceptions.ForEach( e =>
				{
					var exc = c1.MarketingCampaignExceptions.FirstOrDefault( x => x.PKID == e.PKID );
					if( exc == null )
					{
						c1.MarketingCampaignExceptions.Add( new MarketingCampaignException()
						{
							FKMarketingCampaignScreen = e.FKMarketingCampaignScreen,
							Start = e.Start,
							End = e.End
						} );
					}
				} );
			}
			else
			{
				Rp.ExecuteAction( () =>
				{
					var user = ProxylessContext.ContactUsers.Include( x => x.UserRoleMaps.Select( y => y.UserRolePermissionTagMaps ) ).FirstOrDefault( x => x.PKID == userId );
					if( user != null )
					{
						var role = user.UserRoleMaps.FirstOrDefault( x => x.UserRolePermissionTagMaps.Any() );
						if( role != null )
						{
							var permissions = role.UserRolePermissionTagMaps;
							permissions.ForEach(
								x =>
									campaign.MarketingCampaignPermissionsTagMaps.Add( new MarketingCampaignPermissionsTagMap()
									{
										FKPermissionTag = x.FKPermissionTag
									} ) );
						}
					}

					ProxylessContext.MarketingCampaigns.Add( campaign );
				} );
			}

			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
		}

		public void SaveMarketingCampaignScreen( int userId, MarketingCampaignScreen screen )
		{
			if( screen.PKID != 0 )
			{
				var c1 = GetMarketingCampaignScreen( userId, screen.PKID ).Include( x => x.MarketingCampaignScreenSections.Select( y => y.MarketingCampaignScreenSectionImageMaps ) ).FirstOrDefault();
				if( c1 == null )
					return;

				c1.Name = screen.Name;
				c1.Orientation = screen.Orientation;
				c1.Layout = screen.Layout;
				c1.MarketingCampaignScreenSections.ToList()
					.ForEach( sec =>
					{
						sec.MarketingCampaignScreenSectionImageMaps.ToList().ForEach( im =>
						{
							ProxylessContext.MarketingCampaignScreenSectionImageMaps.Remove( im );
						} );
						ProxylessContext.MarketingCampaignScreenSections.Remove( sec );
					} );
				c1.MarketingCampaignScreenSections.Clear();
				screen.MarketingCampaignScreenSections.ForEach( section =>
				{
					var sec = new MarketingCampaignScreenSection
					{
						PKID = section.PKID,
						TransitionDuration = section.TransitionDuration,
						Ordinal = section.Ordinal
					};
					section.MarketingCampaignScreenSectionImageMaps.ForEach( im => sec.MarketingCampaignScreenSectionImageMaps.Add( new MarketingCampaignScreenSectionImageMap
					{
						PKID = im.PKID,
						FKImage = im.FKImage,
						Ordinal = im.Ordinal
					} ) );

					c1.MarketingCampaignScreenSections.Add( sec );
				} );
			}
			else
			{
				Rp.ExecuteAction( () =>
				{
					var user = ProxylessContext.ContactUsers.Include( x => x.UserRoleMaps.Select( y => y.UserRolePermissionTagMaps ) ).FirstOrDefault( x => x.PKID == userId );
					if( user != null )
					{
						var role = user.UserRoleMaps.FirstOrDefault( x => x.UserRolePermissionTagMaps.Any() );
						if( role != null )
						{
							var permissions = role.UserRolePermissionTagMaps;
							permissions.ForEach(
								x =>
									screen.MarketingCampaignScreenPermissionTagMaps.Add( new MarketingCampaignScreenPermissionTagMap()
									{
										FKPermissionTag = x.FKPermissionTag
									} ) );
						}
					}

					ProxylessContext.MarketingCampaignScreens.Add( screen );
				} );
			}

			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
		}
	}
}