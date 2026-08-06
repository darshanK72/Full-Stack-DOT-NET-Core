namespace DataTypesAndVariables.Models;

/*
 * StoreConfig holds values set once at construction time.
 * readonly fields can be assigned only in the constructor (or at declaration).
 */
public class StoreConfig
{
    public readonly string StoreCode;
    public readonly decimal StandardTaxRate;

    public StoreConfig(string storeCode, decimal standardTaxRate)
    {
        StoreCode = storeCode;
        StandardTaxRate = standardTaxRate;
    }
}
