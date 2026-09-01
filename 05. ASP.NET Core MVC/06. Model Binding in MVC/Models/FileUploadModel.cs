/*
 * FILE ROLE: Demonstrates IFormFile binding for single and multiple file uploads
 *            in an MVC form, including validation and safe save patterns.
 * SECTIONS IN THIS FILE:
 *   3a. IFormFile — single file upload
 *   3b. IList<IFormFile> — multiple file uploads
 *   3c. File validation best practices
 *   3d. Mixed payload — file + scalar form fields in one model
 */

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ModelBindingMvc.Models;

/*
 * SECTION 3a: IFormFile — SINGLE FILE UPLOAD
 *
 * IFormFile is the MVC abstraction for an uploaded file. It is ALWAYS bound
 * from the form body ([FromForm] is implicit). The HTTP form MUST use:
 *   <form method="post" enctype="multipart/form-data">
 *   <input type="file" name="ProfilePhoto" />
 *
 * The property name in the model ("ProfilePhoto") matches the <input name> attribute.
 *
 * Key members:
 *   FileName     — the original filename supplied by the browser (UNTRUSTED).
 *                  Never use directly as a file-system path; sanitize or generate
 *                  a server-side name (e.g. Guid.NewGuid() + extension).
 *   Length       — file size in bytes. Check before saving (server-side validation).
 *   ContentType  — MIME type string from the browser (UNTRUSTED — easily spoofed).
 *                  Validate by reading the file header (magic bytes) server-side.
 *   OpenReadStream() — returns a Stream to read content; caller disposes it.
 *   CopyToAsync(stream) — copies the file bytes to a target stream.
 *
 * Nullable IFormFile:
 *   Declare as IFormFile? when the upload is optional. Binding sets it to null
 *   when no file is selected. Non-nullable IFormFile results in a ModelState error
 *   if no file is submitted.
 *
 * PITFALL — large file limits:
 *   By default Kestrel limits request body size to 30 MB and form fields to 128 MB.
 *   Use [RequestSizeLimit] or [RequestFormLimits] on the action to override.
 *   For very large files, disable the form size limit and stream directly from
 *   HttpContext.Request.Body (avoiding buffering the whole file in memory).
 */

/*
 * SECTION 3d: MIXED PAYLOAD — FILE + SCALAR FIELDS
 *
 * A single MVC model can combine scalar form fields and IFormFile properties.
 * All fields come from the same multipart/form-data request body.
 * The binder resolves IFormFile properties via the form file collection and
 * scalar properties via the form value collection — they coexist transparently.
 *
 * HTML form pattern:
 *   <input type="text"  name="DocumentTitle" />
 *   <input type="text"  name="Description" />
 *   <input type="file"  name="MainFile" />
 *   <input type="file"  name="Attachments" multiple />  ← multiple files
 */
public class FileUploadModel
{
    // Scalar fields — bound from the same multipart form as the files
    public string DocumentTitle { get; set; } = string.Empty;
    public string? Description { get; set; }

    /*
     * Single required file — IFormFile (non-nullable) → ModelState error if absent.
     * Add [Required] to surface a friendly validation error via ModelState instead
     * of a NullReferenceException if you plan to use the file unconditionally.
     */
    public IFormFile MainFile { get; set; } = null!; // set by binder; null! suppresses nullable warning

    /*
     * SECTION 3b: IList<IFormFile> — MULTIPLE FILE UPLOADS
     *
     * Declare as IList<IFormFile> or IEnumerable<IFormFile> for a multi-select
     * file input (<input type="file" multiple name="Attachments">).
     *
     * Each selected file becomes one element in the list. The list is empty
     * (not null) when no files are selected.
     *
     * To upload multiple files with DIFFERENT names (separate inputs):
     *   <input type="file" name="Attachments" />   ← first file
     *   <input type="file" name="Attachments" />   ← second file
     *   Both have the same name → collected into the IList<IFormFile>.
     *
     * PITFALL — empty list vs null:
     *   IList<IFormFile> (non-nullable) → initialized to empty list when no files sent.
     *   IList<IFormFile>? (nullable)    → null when no files sent.
     *   Use the non-nullable form and check Count > 0 rather than null.
     */
    public IList<IFormFile> Attachments { get; set; } = new List<IFormFile>();

    /*
     * SECTION 3c: FILE VALIDATION BEST PRACTICES
     *
     * Server-side file validation — check AFTER binding succeeds:
     *
     *   1. Size limit
     *      if (MainFile.Length > 10 * 1024 * 1024)
     *          ModelState.AddModelError(nameof(MainFile), "File must be under 10 MB.");
     *
     *   2. Extension allow-list (content-type is not enough — it can be spoofed)
     *      var allowed = new[] { ".pdf", ".docx", ".png", ".jpg" };
     *      var ext = Path.GetExtension(MainFile.FileName).ToLowerInvariant();
     *      if (!allowed.Contains(ext))
     *          ModelState.AddModelError(nameof(MainFile), "Unsupported file type.");
     *
     *   3. Magic-byte / content sniffing
     *      Read the first few bytes of the stream and compare to known headers:
     *        PDF: 0x25 0x50 0x44 0x46  (%PDF)
     *        PNG: 0x89 0x50 0x4E 0x47  (\x89PNG)
     *      This catches renamed files (.exe renamed to .pdf).
     *
     *   4. Safe file path construction
     *      NEVER use MainFile.FileName as the disk path. Generate a server-side name:
     *        var safeFileName = Guid.NewGuid().ToString("N") +
     *                           Path.GetExtension(MainFile.FileName).ToLowerInvariant();
     *        var savePath = Path.Combine(_uploadPath, safeFileName);
     *
     *   5. Stream the file to disk (avoid storing in memory for large files)
     *      using var stream = System.IO.File.Create(savePath);
     *      await MainFile.CopyToAsync(stream);
     *
     * These validations are performed in the action method (OrdersController.Upload).
     */
}
