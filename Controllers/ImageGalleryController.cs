using ConnectCMS.Models.Image;
using ConnectCMS.Models.ImageGallery;
using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class ImageGalleryController : ControllerBase
	{
		[HttpPost]
		public JsonNetResult ImageGalleryLoadGallery( string id, string type )
		{
			var galleries = new List<ImageGalleryModel>();
			var tags = new List<TagModel>();
			switch( type )
			{
				case "upload":
					tags = ConnectCmsRepository.ImageRepository.GetTags();
					break;

				case "browse":
					var stockGallery = new ImageGalleryModel
					 {
						 Name = "Stock Images",
						 Id = "Stock",
						 Type = "Stock",
						 SubGalleries = new List<ImageGalleryModel>()
					 };

					var customGallery = new ImageGalleryModel
					{
						Name = "Custom Images",
						Id = "Custom",
						Type = "Custom"
					};

					galleries.Add( stockGallery );
					galleries.Add( customGallery );
					break;

				case "Stock":
					var catTags = ConnectCmsRepository.ImageRepository.GetStockCategoryTags();

					if( catTags.Any() )
					{
						galleries.AddRange( catTags.Select( catTag => new ImageGalleryModel
						{
							Id = catTag.Tag.PKID.ToString( CultureInfo.InvariantCulture ),
							Name = catTag.Tag.Name,
							GalleryImageUrl = catTag.CatImageUrl,
							Type = "Tag"
						} ) );
					}
					break;

				case "Custom":
					break;

				case "Enterprise":
					break;

				case "Tag":
					break;
			}

			return JsonNet( new { Galleries = galleries, Tags = tags.OrderBy( x => x.Name ) } );
		}

		[HttpPost]
		public JsonNetResult ImageGalleryLoadGalleryImages( string id, string type, int? page, int? pageSize, List<TagModel> selectedTags, ImageGalleryImageContraints imageContraints )
		{
			var images = new List<ImageModel>();
			if( selectedTags == null )
				selectedTags = new List<TagModel>();

			switch( type )
			{
				case "Stock":
					var stags = selectedTags.Select( x => x.PKID ).ToArray();
					var stockImages = ConnectCmsRepository.ImageRepository.GetStockImages();
					images.AddRange( stockImages.Where( x => stags.All( s => x.Tags.Any( t => t.PKID == s ) ) ) );
					break;

				case "Custom":
					var customImages = ConnectCmsRepository.ImageRepository.GetCustomImages();
					images.AddRange( customImages );
					break;

				case "Tag":
					var tagId = int.Parse( id );
					var tagImages = ConnectCmsRepository.ImageRepository.GetTagImages( tagId );
					images.AddRange( tagImages );
					break;
			}

			if( imageContraints != null )
				images.RemoveAll(
					x =>
						( imageContraints.MinWidth.HasValue && x.Width < imageContraints.MinWidth ) ||
						( imageContraints.MinHeight.HasValue && x.Height < imageContraints.MinHeight ) );

			var imageCount = images.Count;

			var tagsUsed = new List<TagModel>();
			foreach( var t in images.Where( imageMapModel => imageMapModel.Tags != null ).SelectMany( imageMapModel => imageMapModel.Tags ).GroupBy( g => g.PKID ) )
			{
				var selected = selectedTags.FirstOrDefault( x => x.PKID == t.Key );
				if( selected == null )
				{
					var x = t.First();
					x.UsageCount = t.Count();
					tagsUsed.Add( x );
				}
				else
				{
					selected.UsageCount = t.Count();
				}
			}

			if( pageSize.HasValue && page.HasValue )
			{
				var pageStart = page.Value * pageSize.Value;
				var pageCount = pageSize.Value <= 0 ? 1 : pageSize.Value;

				if( images.Count <= pageStart )
					images = new List<ImageModel>();
				else if( images.Count >= pageStart + pageCount )
					images = images.GetRange( pageStart, pageCount );
				else if( images.Count < pageStart + pageCount )
					images = images.GetRange( pageStart, images.Count - pageStart );
			}

			return JsonNet( new
			{
				Images = images,
				ImageCount = imageCount,
				Tags = tagsUsed.OrderBy( x => x.Name ),
				SelectedTags = selectedTags
			} );
		}

		[HttpPost]
		public JsonNetResult ImageGalleryUploadImages( string data )
		{
			var maps = ConnectCmsRepository.ImageRepository.UploadImages( data, Request );

			return JsonNet( maps );
		}
	}
}