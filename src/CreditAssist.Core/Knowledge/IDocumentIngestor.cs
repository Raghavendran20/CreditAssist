namespace CreditAssist.Core.Knowledge;

public record IngestionResult(string FileName, int Pages, int Chunks);

public interface IDocumentIngestor
{
    Task<IngestionResult> IngestPdfAsync(Stream pdf, string fileName, CancellationToken ct = default);
}