using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public, ApplyToStateMachine = false )]
	public class EntityController : ControllerBase
	{
		//
		// GET: /Entity/
		public JsonNetResult Update( string entityKey, string property, object value )
		{
			var result = ConnectCmsRepository.EntityRepository.UpdateEntityProperty( entityKey, property, value );

			return JsonNet( result );
		}
	}
}