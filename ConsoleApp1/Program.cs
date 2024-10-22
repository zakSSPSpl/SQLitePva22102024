using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        SqliteConnectionStringBuilder builder = new() { DataSource = @"c:\Users\novotny.an.2022\produkty.db", Mode = SqliteOpenMode.ReadWriteCreate };

        using (IDbConnection db = new SqliteConnection(builder.ToString()))
        {
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = """
                DROP TABLE IF EXISTS Vyrobci;
                DROP TABLE IF EXISTS Produkty;

                CREATE TABLE Vyrobci(
                    id INTEGER NOT NULL PRIMARY KEY  AUTOINCREMENT,
                    nazev TEXT NOT NULL
                );

                CREATE TABLE Produkty(
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    nazev TEXT NOT NULL,
                    serial TEXT NOT NULL,
                    popis TEXT,
                    datum_naskladneni TEXT NOT NULL,
                    vyrobce INTEGER NOT NULL,
                    cena INTEGER NOT NULL,
                    FOREIGN KEY(vyrobce) REFERENCES Vyrobci(id)
                );                
                """;

            cmd.ExecuteNonQuery();

            db.Close();
        }


    }

    public static bool InsertIntoDB(IDbConnection dbConnection, Product newProduct)
    {
        dbConnection.Open();

        IDbCommand cmd = dbConnection.CreateCommand();

        cmd.Parameters["@cnazev"] = newProduct.Name;
        cmd.Parameters["@cserial"] = newProduct.Serial;
        cmd.Parameters["@cpopis"] = newProduct.Description;
        cmd.Parameters["@cdatum_naskladneni"] = newProduct.Inbound.ToString();
        cmd.Parameters["@cvyrobce"] = newProduct.Producer;
        cmd.Parameters["@ccena"] = newProduct.Price.ToString();


        cmd.CommandText = """
            INSERT INTO Produkty(nazev, serial, popis, datum_naskladneni, vyrobce, cena)
                           VALUES(@cnazev, @cserial, @cpopis, @cdatum_naskladneni, @cvyrobce, @ccena);
            """;

        try
        {
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        finally
        {
            dbConnection.Close();
        }
    }

    public static void WriteOutProducts(IDbConnection dbConnection, string serialNumber)
    {
        dbConnection.Open();

        IDbCommand cmd = dbConnection.CreateCommand();

        cmd.Parameters["@cserial"] = serialNumber;

        cmd.CommandText = """
            SELECT * FROM Produkty
            WHERE Produkty.serial=@cserial;
            """;

        var output = cmd.ExecuteReader();

        if (output != null)
            while (output.Read())
            {
                Console.WriteLine($"#{output["id"]} - {output["nazev"]} (SČ: {output["serial"]}, výrobce {output["vyrobce"]}, naskladněno {DateTime.Parse(output["datum_naskladneni"].ToString() ?? "")}, cena {output["cena"]}, \"{output["popis"]}\")");
            }
    }

    public static bool DeleteProduct(IDbConnection dbConnection, long id)
    {
        dbConnection.Open();

        IDbCommand cmd = dbConnection.CreateCommand();

        cmd.Parameters["@cid"] = id.ToString();

        cmd.CommandText = """
            DELETE FROM Produkty
            WHERE id=@cid;
            """;

        try
        {
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        finally
        {
            dbConnection.Close();
        }
    }

    public static bool UpdateItem(IDbConnection dbConnection, Product product, long id)
    {
        dbConnection.Open();

        IDbCommand cmd = dbConnection.CreateCommand();

        cmd.Parameters["@cid"] = id.ToString();
        cmd.Parameters["@cnazev"] = product.Name;
        cmd.Parameters["@cserial"] = product.Serial;
        cmd.Parameters["@cpopis"] = product.Description;
        cmd.Parameters["@cdatum_naskladneni"] = product.Inbound.ToString();
        cmd.Parameters["@cvyrobce"] = product.Producer;
        cmd.Parameters["@ccena"] = product.Price.ToString();

        cmd.CommandText = """
            UPDATE Produkty
            SET
                nazev = @cnazev,
                serial = @cserial,
                popis = @cpopis,
                datum_naskladneni = @cdatum_naskladneni,
                vyrobce = @cvyrobce,
                cena = @ccena
            WHERE id = @cid;
            """;

        try
        {
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        finally
        {
            dbConnection.Close();
        }
    }

    public static void AlterProducts(IDbConnection dbConnection)
    {
        dbConnection.Open();

        IDbCommand cmd = dbConnection.CreateCommand();
    }

    public class Product
    {
        public Product(string name, string serial, string description, DateTime inbound, long producer, long price)
        {
            Name = name;
            Serial = serial;
            Description = description;
            Inbound = inbound;
            Producer = producer;
            Price = price;
        }

        public string Name { get; set; }
        public string Serial { get; set; }
        public string Description { get; set; }
        public DateTime Inbound { get; set; }
        public long Producer { get; set; }
        public long Price { get; set; }
    }
}