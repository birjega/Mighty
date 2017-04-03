using System;

namespace Mighty
{
	// Abstract class 'interface' for ORM and ADO.NET Data Access Wrapper methods.
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	public abstract class MicroORM
	{
		// We need the schema so we can instantiate from submit (or any other namevaluecollection-ish thing via ToExpando), to match columns
		//...

		// We can implement prototype and defaultvalue(column)
		//...

		abstract public DbConnection OpenConnection();

		abstract public IEnumerable<dynamic> Query(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public IEnumerable<dynamic> Query(string sql,
			params object[] args);
		abstract public IEnumerable<dynamic> QueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public IEnumerable<dynamic> QueryFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultiple(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultiple(string sql,
			params object[] args);
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultipleWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultipleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public int Execute(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public int Execute(string sql,
			params object[] args);
		// COULD add a RowCount class, like Cursor, to pick out the rowcount if required
		abstract public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public dynamic ExecuteAsProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public object Scalar(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public object Scalar(string sql,
			params object[] args);
		abstract public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		// use this also for MAX, MIN, SUM, AVG (basically it's scalar on current table)
		// NB returns object because of MySQL ulong
		abstract public object Count(string expression = "COUNT(*)", string where = "",
			params object[] args);

		// non-ORM (NB columns is only used in generating SQL, so makes no sense on either of these)
		abstract public dynamic Paged(string sql,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);
		abstract public dynamic PagedFromProcedure(string spName,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		// ORM
		abstract public dynamic Paged(string where = "", string orderBy = "",
			string columns = "*", int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		// ORM
		abstract public IEnumerable<dynamic> All(
			string where = "", string orderBy = "", int limit = 0, string columns = "*",
			params object[] args);
		abstract public IEnumerable<dynamic> AllWithParams(
			string where = "", string orderBy = "", int limit = 0, string columns = "*",
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		// ORM: Single from our table
		abstract public dynamic Single(object key, string columns = "*");
		// there are really tricky reasons not to have this, aren't there? Are there? What are they?
		abstract public dynamic Single(string where, string columns = "*", params object[] args);		
		// WithParams version just in case, allows transaction for a start
		abstract public dynamic SingleWithParams(string where, string columns = "*",
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);


		// does update OR insert, per item
		// in my NEW version, null or default value for type in PK will save as new, as well as no PK field
		// only we don't know what the pk type is... but we do after getting the schema, and we should just use = to compare without worrying too much about types
		// is checking whether every item is valid before any saving - which is good - and not the same as checking
		// something at inserting/updating time; still if we're going to use a transaction ANYWAY, and this does.... hmmm... no: rollback is EXPENSIVE
		// returns the sum of the number of rows affected;
		// *** insert WILL set the PK field, as long as the object was an expando in the first place (could upgrade that; to set PK
		// in Expando OR in settable property of correct name)
		// *** we can assume that it is NEVER valid for the user to specify the PK value manually - though they can of course specify the pkFieldName,
		// and the pkSequence, for those databases which work that way; I strongly suspect we should be able to shove the sequence select into ONE round
		// trip to the DB, as well.
		// (Of course, this would mean that there would be no such thing as an ORM provided update, for a table without a PK. You know what? It *is* valid to
		// set - and update based on - a compound PK. Which means it must BE valid to set a non-compound PK.)
		// I think we want primaryKeySequence (for dbs which use that; defaults to no sequence) and primaryKeyRetrievalFunction (for dbs which use that; defaults to
		// correct default to DB, but may be set to null). If both are null, you can still have a (potentially compound) PK.
		// We can use INSERT seqname.nextval and then SELECT seqname.currval in Oracle.
		// And INSERT nextval('seqname') and then currval('seqname') (or just lastval()) in PostgreSQL.
		// (if neither primaryKeySequence nor primaryKeyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		// *** okay, shite, how do we know if a compound key object is an insert or an update? I think we just provide Save, which is auto, but can't work for manual primary keys,
		// and Insert and Update, which will do what they say on the tin, and which can.

		// Cannot be used with manually controlled primary keys (which includes compound primary keys), as the microORM cannot tell apart an insert from an update in this case
		// but I think this can just be an exception, as we really don't need to worry most users about it.
		// exception can check whether we are compound; or whether we may be sequence, but just not set; or whether we have retrieval fn intentionally overridden to empty string;
		// and give different messages.
		abstract public int Save(params object[] items);
		
		abstract public int Insert(params object[] items);
		abstract public int Update(params object[] items);

		// returns expando with the PK set
		// We don't need this: (since save sets the PK anyway)
		//abstract public dynamic Insert(object o);

		// apply fields which are present to row matching key
		abstract public int UpdateFrom(object o, object key);

		// apply fields which are present to rows matching where clause
		abstract public int UpdateFrom(object o, string where = "1=1", params object[] args);

		// delete item from table; what about deleting by object? (maybe key can be pk OR expando containing pk? no)
		// also why the f does this fetch the item back before deleting it, when it's by PK? sod it, let the user
		// fetch it; only delete by item, and only if (there's a PK and) the item contains the PK. that means
		// the user has prefetched it. Good.
		// I prefer this:
		abstract public int Delete(params object[] items);
		abstract public int DeleteKeys(params object[] keys);
		// This called with no args will delete the entire table. Which is correct. Okay, it's too scary, they have to do Delete("1=1");
		abstract public int Delete(string where = "1=0", params object[] args);

		// We also have validation, called on each object to be updated, before any saves, if a validator was passed in
		//...

		/// Hooks; false return => do nothing with this object but continue with the list
		public bool Inserting(dynamic item) { return true; }
		public void Inserted(dynamic item) {}
		public bool Updating(dynamic item) { return true; }
		public void Updated(dynamic item) {}
		public bool Deleting(dynamic item) { return true; }
		public void Deleted(dynamic item) {};

		abstract public DbCommand CreateCommand(string sql,
			DbConnection conn = null, // do we need (no) or want (not sure) this, here? it is a prime purpose of a command to have a connection, so why not?
			params object[] args);
		abstract public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args);

		// kv pair stuff for dropdowns, but it's not obvious you want your dropdown list in kv pair...
		// it's a lot of extra code for this - you could add to kvpairs (whatever it's called) as
		// an extension of IEnumerable<dynamic> ... if you can. That means almost no extra code.
		// it is very easy for the user to do this conversion themselves

		// create item from form post, only filling in fields which are in the schema - not bad!
		// (but the form post namevaluecollection is not in NET CORE1.1 anyway ... so what are they doing?
		// no form posts per se in MVC, but what about that way I was reading back from a form, for files?)
		// Oh bollocks, it was left out by mistake and a I can have it:
		// https://github.com/dotnet/corefx/issues/10338

		//For folks that hit missing types from one of these packages after upgrading to Microsoft.NETCore.UniversalWindowsPlatform they can reference the packages directly as follows.
		//"System.Collections.NonGeneric": "4.0.1",
		//"System.Collections.Specialized": "4.0.1", ****
		//"System.Threading.Overlapped": "4.0.1",
		//"System.Xml.XmlDocument": "4.0.1"

		// schema retrieval stuff ...
		// when and where is it used?

		public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
	}
}
