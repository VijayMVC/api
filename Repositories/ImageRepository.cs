using ConnectCMS.Models.Image;
using ConnectCMS.Models.ImageGallery;
using ConnectCMS.MonsciergeImaging;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ConnectCMS.Repositories
{
	public class ImageRepository : ChildRepository
	{
		public ImageRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public ImageRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public void DeleteEnterpriseCustomImage( int deviceId, int enterpriseId, int imageId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
		}

		public void DeleteEnterpriseStockImage( int deviceId, int enterpriseId, int imageId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
		}

		public List<ImageModel> GetCustomImages()
		{
			var currentUser = RootRepository.SecurityRepository.GetLoggedInUser();

			return Rp.ExecuteAction( () => ( from ci in Context.Images
											 where ci.IsActive && ci.FKAccount == currentUser.FKAccount && ci.FKAccount != null
											 select ci ) ).ToList().Select( i => new ImageModel( i ) ).ToList();
		}

		public List<CategoryTagModel> GetStockCategoryTags()
		{
			return Rp.ExecuteAction( () => ( from t in Context.Tags
											 join c in Context.Categories on t.REFOBJID equals c.REFOBJID
											 join cm in Context.CategoryMaps on c.PKID equals cm.FKChildCategory
											 where cm.FKParentCategory == null && t.ImageTagMaps.Count > 0 && cm.IsActive && c.IsActive
											 select
												 new CategoryTagModel
												 {
													 Tag = new TagModel { FKAccount = t.FKAccount, Name = t.Name, PKID = t.PKID, REFOBJID = t.REFOBJID },
													 CatImageUrl =
														 cm.Image == null
															 ? t.ImageTagMaps.FirstOrDefault() == null
																 ? string.Empty
																 : t.ImageTagMaps.FirstOrDefault().Image.Path
															 : cm.Image.Path
												 } ) ).ToList();
		}

		public List<ImageModel> GetStockImages()
		{
			return Rp.ExecuteAction( () => ( from si in Context.Images
											 let tags = si.ImageTagMaps.Select( t => t.Tag )
											 where si.IsActive && si.FKAccount == null
											 select new
											 {
												 Image = si,
												 Tags = tags
											 } ) ).ToList().Select( i => new ImageModel( i.Image )
								{
									Tags = i.Tags.Select( t => new TagModel( t ) ).ToList()
								} ).ToList();
		}

		public List<ImageModel> GetTagImages( int tagId )
		{
			return Rp.ExecuteAction( () => ( from si in Context.Images
											 let tags = si.ImageTagMaps.Select( t => t.Tag )
											 where si.IsActive && si.ImageTagMaps.Any( x => x.FKTag == tagId )
											 select new
											 {
												 Image = si,
												 Tags = tags
											 } ) ).ToList().Select( ti => new ImageModel( ti.Image )
											  {
												  Tags = ti.Tags.Select( t => new TagModel( t ) ).ToList()
											  } ).ToList();
		}

		public List<TagModel> GetTags()
		{
			return Rp.ExecuteAction( () => ( from t in Context.Tags
											 select t ) ).ToList().Select( t => new TagModel( t ) ).ToList();
		}

		public void UpdateEnterpriseCategoryMapCustomImage( int deviceId, int enterpriseId, int imageId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
		}

		public void UpdateEnterpriseCategoryMapStockImage( int deviceId, int enterpriseId, int imageId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
		}

		public List<ImageModel> UploadImages( string data, HttpRequestBase request )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var images = new List<ImageModel>();
			var json_serializer = new JavaScriptSerializer();
			var imageData = json_serializer.Deserialize<ImageGalleryFileData[]>( data );

			for( var i = 0; i < request.Files.Count; i++ )
			{
				var file = request.Files[ i ]; //Uploaded file
				//Use the following properties to get file's name, size and MIMEType
				if( file != null )
				{
					//var fileSize = file.ContentLength;
					var fileName = file.FileName;
					//var mimeType = file.ContentType;
					var fileContent = file.InputStream;

					var fileBytes = new byte[ fileContent.Length ];
					fileContent.Read( fileBytes, 0, ( int )fileContent.Length );

					var client = new ImageManagementServiceSoapClient();
					var result = client.UploadImage( fileName, fileBytes );

					var iData = imageData[ i ];

					Image sI;

					if( iData.IsStockImage )
					{
						sI = new Image { Path = result, Name = fileName, Width = iData.Width, Height = iData.Height };
						Context.Images.Add( sI );
						Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
						images.Add( new ImageModel( sI ) );
					}
					else
					{
						sI = new Image { Path = result, Name = fileName, Width = iData.Width, Height = iData.Height, FKAccount = user.FKAccount };
						Context.Images.Add( sI );
						Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
						images.Add( new ImageModel( sI ) );
					}

					if( iData.IsStockImage && iData.Tags != null && iData.Tags.Count > 0 )
					{
						var tagIds = new List<int>();
						foreach( var tag in iData.Tags )
						{
							if( !tag.Id.HasValue )
							{
								var t = Rp.ExecuteAction( () => ( from tags in Context.Tags
																  where tags.Name == tag.Tag
																  select tags.PKID ).FirstOrDefault() );

								if( t == 0 )
								{
									var newTag = new Tag { Name = tag.Tag };
									Context.Tags.Add( newTag );
									Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
									tag.Id = newTag.PKID;
								}
							}
							if( tag.Id.HasValue )
								tagIds.Add( tag.Id.Value );
						}

						var imgMaps = sI.ImageTagMaps.ToList();

						foreach( var imgTag in imgMaps )
						{
							var exists = iData.Tags.FirstOrDefault( x => x.Id == imgTag.FKTag );

							if( exists != null )
							{
								iData.Tags.Remove( exists );
								continue;
							}

							if( iData.Tags.All( x => x.Id != imgTag.FKTag ) )
							{
								sI.ImageTagMaps.Remove( imgTag );
							}
						}

						foreach( var tag in iData.Tags )
						{
							if( tag.Id != null )
								sI.ImageTagMaps.Add( new ImageTagMap { FKTag = tag.Id.Value } );
						}

						Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
					}
				}
			}

			return images;
		}

		public Image GetImage( int imageId )
		{
			var image = ProxylessContext.Images.FirstOrDefault( x => x.PKID == imageId );
			if( image == null )
				throw new Exception( "The image you are trying to get doesn't exist" );
			return image;
		}

		public Image InsertImage( string path, string name, int width, int height )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var image = ProxylessContext.Images.FirstOrDefault( x => x.Path == path && x.Width == width && x.Height == height && x.FKAccount == user.FKAccount );
			if( image != null )
				return image;

			image = new Image
			{
				FKAccount = user.FKAccount,
				Path = path,
				Name = name,
				Width = width,
				Height = height
			};

			ProxylessContext.Images.Add( image );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			return image;
		}

		public static string BuildImagePath( Image image )
		{
			return string.Format( "{0}?filename={1}&width={2}&height={3}",
				Parser.GetAppSetting( "ImageServiceUrl", UrlHelper.FormatBaseUri( "/MonsciergeImaging/getImage.ashx" ) ), image.Path,
				image.Width, image.Height );
		}
	}
}