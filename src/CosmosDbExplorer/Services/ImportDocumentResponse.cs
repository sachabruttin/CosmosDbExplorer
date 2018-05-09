namespace CosmosDbExplorer.Services
{
    public class ImportDocumentResponse
    {
        public long TotalNumberOfDocumentsInserted { get; set; }
        public double TotalRequestUnitsConsumed { get; set; }
        public double TotalTimeTakenSec { get; set; }
    }
}
