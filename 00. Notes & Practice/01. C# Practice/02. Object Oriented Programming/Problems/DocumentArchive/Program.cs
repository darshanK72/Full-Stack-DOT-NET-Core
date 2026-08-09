/*
 * PROBLEM: Document Archive
 *
 * Compliance stores PDFs and text memos. Some entries export to bytes; all
 * support keyword search. Archive service processes mixed document types
 * through shared abstractions.
 *
 * This exercise covers:
 *   ch06 — abstract class; multiple interfaces; explicit interface implementation
 *   ch09 — document archive real-world example
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace LegalArchive
{
    interface IExportable
    {
        byte[] Export();
    }

    interface ISearchable
    {
        bool ContainsKeyword(string keyword);
    }

    /*
     * Storage key distinct from human-readable Title — requires explicit implementation.
     */
    interface IIndexedItem
    {
        string Title { get; }
    }

    abstract class Document
    {
        public string Title { get; }
        public DateTime CreatedUtc { get; }

        protected Document(string title, DateTime createdUtc)
        {
            // TODO: validate non-empty title
            throw new NotImplementedException();
        }

        public abstract string DocumentType { get; }

        public abstract string RenderPreview(int maxChars);

        public virtual int WordCount
        {
            get
            {
                // TODO: split RenderPreview(int.MaxValue) on whitespace; empty -> 0
                throw new NotImplementedException();
            }
        }

        protected static string Truncate(string text, int maxChars)
        {
            if (maxChars <= 0 || text.Length <= maxChars)
                return text;
            return text.Substring(0, maxChars) + "...";
        }
    }

    class PdfDocument : Document, IExportable, ISearchable, IIndexedItem
    {
        public int PageCount { get; }

        public override string DocumentType => "PDF";

        public PdfDocument(string title, DateTime createdUtc, int pageCount)
            : base(title, createdUtc)
        {
            // TODO: validate pageCount > 0
            throw new NotImplementedException();
        }

        public override string RenderPreview(int maxChars)
        {
            // TODO: "PDF:{Title} ({PageCount} pages)" then truncate
            throw new NotImplementedException();
        }

        public byte[] Export()
        {
            // TODO: UTF-8 bytes of preview (max 200 chars)
            throw new NotImplementedException();
        }

        public bool ContainsKeyword(string keyword)
        {
            // TODO: case-insensitive search on title + "PDF"
            throw new NotImplementedException();
        }

        string IIndexedItem.Title
        {
            get
            {
                // TODO: deterministic storage key e.g. DOC-{CreatedUtc:yyyyMMdd}-{hash}
                throw new NotImplementedException();
            }
        }
    }

    class TextMemo : Document, IExportable, ISearchable, IIndexedItem
    {
        public string Body { get; }

        public override string DocumentType => "MEMO";

        public TextMemo(string title, DateTime createdUtc, string body)
            : base(title, createdUtc)
        {
            Body = body ?? string.Empty;
        }

        public override string RenderPreview(int maxChars)
        {
            // TODO: first line of body or "(empty)"; truncate per rules
            throw new NotImplementedException();
        }

        public byte[] Export()
        {
            // TODO: UTF-8 bytes of preview (max 200 chars)
            throw new NotImplementedException();
        }

        public bool ContainsKeyword(string keyword)
        {
            // TODO: case-insensitive search on title + body
            throw new NotImplementedException();
        }

        string IIndexedItem.Title
        {
            get
            {
                // TODO: deterministic storage key — same format as PdfDocument
                throw new NotImplementedException();
            }
        }
    }

    class ArchiveService
    {
        private readonly List<Document> _documents = new List<Document>();

        public IReadOnlyList<Document> All => _documents.AsReadOnly();

        public bool Add(Document doc)
        {
            // TODO: false if another doc with same public title (case-insensitive)
            throw new NotImplementedException();
        }

        public List<byte[]> ExportAll(IExportable filter)
        {
            // TODO: export every stored doc implementing IExportable (cast safely)
            throw new NotImplementedException();
        }

        public List<Document> Search(string keyword)
        {
            // TODO: docs implementing ISearchable where ContainsKeyword is true
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: add one PDF and one memo; search and export
            // TODO: print previews and explicit IIndexedItem.Title via cast
            throw new NotImplementedException();
        }
    }
}
