namespace Cllearworks.COH.BusinessManager
{
    public interface IMappingFactory<TDataModel, TListModel, TModel>
    {
        TListModel ConvertToListModel(TDataModel dataModel);
        TModel ConvertToModel(TDataModel dataModel);
        TDataModel ConvertToDataModel(TModel model);
    }
}
