using System;

namespace AsyncAdoNet.Models;

/*
 * FILE ROLE:
 *   Row type mapped from dbo.Products columns — used by ExecuteReader demos
 *   and the IAsyncEnumerable preview stream.
 *
 * SECTIONS IN THIS FILE:
 *   1. When async ADO.NET helps + ProductRow record
 */

/*
 * SECTION 1: WHEN ASYNC ADO.NET HELPS
 *
 * | Situation                         | Sync ADO.NET              | Async ADO.NET                |
 * |-----------------------------------|---------------------------|------------------------------|
 * | ASP.NET Core request handler      | Blocks a thread per wait  | Thread serves other requests |
 * | Many parallel DB calls            | Thread pool pressure      | Fewer blocked threads        |
 * | UI app (WPF/WinForms)             | Can freeze UI if misused  | await keeps UI responsive    |
 * | CPU-heavy in-memory LINQ on rows  | Async does NOT help       | Use sync or parallel CPU     |
 *
 * Rule: use async all the way -- async controller -> async service -> async ADO.NET.
 * Do not call .Result or .Wait() on database Tasks (deadlock risk with contexts).
 *
 * ProductRow maps the four columns selected in reader and streaming demos below.
 * -------------------------------------------------------------------------
 */
public sealed record ProductRow(int ProductId, string Name, decimal UnitPrice, int Stock);
