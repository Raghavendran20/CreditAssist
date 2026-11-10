using CreditAssist.Core.Knowledge;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace CreditAssist.Infrastructure.Documents;

public sealed class PdfDocumentIngestor(IPolicyIndexer indexer) : IDocumentIngestor
{
    private const int MaxChars = 800;
    private const int OverlapChars = 150;

    public async Task<IngestionResult> IngestPdfAsync(Stream pdf, string fileName, CancellationToken ct = default)
    {
        // PdfPig needs a seekable stream; uploaded streams usually are not
        using var buffer = new MemoryStream();
        await pdf.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        var chunks = new List<PolicySnippet>();
        var docName = Path.GetFileNameWithoutExtension(fileName);
        int pageCount;

        using (var document = PdfDocument.Open(buffer))
        {
            pageCount = document.NumberOfPages;

            foreach (var page in document.GetPages())
            {
                // Extracts text in reading order, keeping line breaks
                var pageText = ContentOrderTextExtractor.GetText(page);
                var pieces = TextChunker.Split(pageText, MaxChars, OverlapChars);

                for (int i = 0; i < pieces.Count; i++)
                {
                    chunks.Add(new PolicySnippet(
                        Id: $"{fileName}#p{page.Number}-c{i + 1}",     // deterministic id
                        Title: $"{docName}, page {page.Number}",
                        Text: pieces[i],
                        Source: fileName,
                        Page: page.Number));
                }
            }
        }

        if (chunks.Count == 0)
            throw new InvalidOperationException(
                "No text found in this PDF. Scanned (image-only) PDFs need OCR, which is not supported yet.");

        // Replace any previous version of this file
        await indexer.DeleteBySourceAsync(fileName, ct);
        var indexed = await indexer.IndexAsync(chunks, ct);

        return new IngestionResult(fileName, pageCount, indexed);
    }
}