# ![logo](https://mightyorm.github.io/Mighty/assets/realfavicon/favicon-32x32.png) Mighty

[![NuGet](https://img.shields.io/nuget/v/Mighty.svg)](https://nuget.org/packages/Mighty)
[![Documentation](https://img.shields.io/badge/Documentation-f52a62.svg)](https://mightyorm.github.io/Mighty/)
[![Change Log](https://img.shields.io/badge/Change%20Log-green)](CHANGELOG.md)

Available for:

 - .NET Core 1.0+ (including 3.0 final)
 - .NET Framework 4.5+
 - .NET Framework 4.0+ (without async support)

---

Mighty features classic Massive-style sweetness.

It really is simple and sweet, with *no* other setup these lines just do what they say:

```c#
var db = new MightyOrm(connectionString, "Film", "FilmID");

var film = db.Single(47);
film.Description = "This is a better description";
db.Save(film);

var films = db.All(new { Director = "Spielberg" });
foreach (var film in films) Console.WriteLine(film.Title);
```

But now extended - hopefully just as sweetly! - in several new directions.

# Feature Overview

## Named and directional parameters

```c#
var result = db.ExecuteProcedure("my_add_proc",
    inParams: new { a = 1, b = 2}, outParams: new { c = (int?)null });
Console.WriteLine(result.c); // 3
```

Also with support for named input-output and return parameter types; and these can even be mixed with classic Massive-style auto-numbered parameters (`@0`, `@1`, etc. where you provide only the values).

## Optional strong typing

Mighty now supports generic types:

```c#
var db = new MightyOrm<Film>(connectionString, primaryKeys: "FilmID");
var films = db.All();
foreach (Film film in films)
{
    Console.WriteLine($"{film.Title}: {film.Description}");
}
```

## Table and column name mapping

Recently added, support for function based and attribute based field and class to column and table name mapping. Here's a quick attribute based example:

```c#
[DatabaseTable("films")]
public class Film
{
    [DatabaseColumn("film_id")]
    public int FilmID;

    [DatabaseColumn("film_title")]
    public int Title;
}
```


## Multiple result sets

Hot off the press!

As well as dynamic multiple results sets, Mighty now properly supports strongly typed multiple result sets:

```c#
var db = new MightyOrm(connectionString);
var now = DateTime.Now;
using (var multiple = db.ExecuteMultipleFromProcedure("PurchaseReport",
    inParams: new { StartDate = now.AddMonths(6), EndDate = now })
{
    multiple.NextResultSet();
    foreach (var summary in multiple.CurrentResultSet.ResultsAs<PurchaseReportSummary>())
    {
        Console.WriteLine($"Total Sales for Report Period: ${summary.Total}");
    }

    multiple.NextResultSet();
    foreach (var monthly in multiple.CurrentResultSet.ResultsAs<PurchaseReportMonthly>())
    {
        Console.WriteLine($"Total Sales for Month ${monthly.Month}: ${monthly.Total}");
    }
}
```

## Cursor parameter support

Unique to Mighty!

Cursor support (on Oracle and PostgreSQL - the only two supported databases which will pass cursors out to client code):


```c#
var results = db.ExecuteWithParams("begin open :p_rc for select * from emp where deptno = 10; end;",
    outParams: new { p_rc = new Cursor() },
    // shared connection (Oracle) or transaction (PostgreSQL) required to share cursors
    connection: conn);
db.ExecuteAsProcedure("cursor_in_out.process_cursor",
    inParams: new { p_cursor = results.p_rc },
    connection: conn);
```

## Cursor dereferencing

Oracle ADO.NET provider for .NET automatically supports cursor dereferencing: when you query from a result set which contains cursors, what you actually get back is multiple result sets, one for each cursor.

Npgsql for PostgreSQL used to (only partially) support this, but [currently no longer does](https://github.com/npgsql/npgsql/issues/1785). In the mean time, if you need this, it is fully supported in Mighty! This is potentially very useful since it makes it possible to return multiple resultsets from stored procedures in PostgreSQL. It is not the most efficient way to pass result sets on PostrgeSQL. But it is the only way to pass multiple result sets, and it is simple to set up and easy to code against!

## Transactions

All database access methods on Mighty accept a `DbConnection`.  Unlike Dapper you don't *have* to do this, and in fact, you can spend most of your life in Mighty avoiding directly using `System.Data.Common` at all. But when you do need transactions, now you can:

```c#
var db = new MightyOrm(connectionString);
using (var connection = db.OpenConnection())
{
    using (var trans = conn.BeginTransaction())
    {
        var customer = db.Insert(CustomerInfo, connection);
        OrderInfo.CustomerID = customer.CustomerID;
        db.Insert(OrderInfo, connection);
        trans.Commit();
    }
}
```

## Paging support

Writing paging SQL ought to be simple, right? On `LIMIT`-`OFFSET` databases it is. On `ROW_NUMBER()` databases (SQL Server and Oracle), it's really not. Fortunately, Mighty writes it for you!

```c#
var films = new MightyOrm(connectionString, "Film");
var page = films.Paged(orderBy: "Title", currentPage: 2, pageSize: 30);
```

Mighty also has a `PagedFromSelect` method which, apart from just doing what it says on the tin, allows you to simply and correctly page over joined table results, if that is what you need!

## Supported databases

 - SQL Server
 - MySQL
 - PostgreSQL
 - Oracle
 - SQLite

## Documentation

Here is the rest of the [![Documentation](https://img.shields.io/badge/Documentation-f52a62.svg)](https://mightyorm.github.io/Mighty/) for Mighty.

Here is the [![Change Log](https://img.shields.io/badge/Change%20Log-green)](CHANGELOG.md).
