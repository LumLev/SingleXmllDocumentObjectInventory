// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;

Inventory TI = new Inventory();

doAgain:
Console.WriteLine("\n?= A = add item; E = erase item; C = check item; X = AddQuantity; Q = quit");
var x = Console.ReadKey();
switch (x.KeyChar)
{
    case 'A': TI.AddItem( new StoredItem { Id = "a" + DateTime.Now.ToString("FFFFFFFddMMyyyyhhmmss"), Name = "SOmeItem", Quantity = 0 }); break;
    case 'E': bool xA = TI.RemoveItem(TheIdRequest()); Console.WriteLine(xA); break;
    case 'C': StoredItem sI = TI.CheckItem(TheIdRequest()); Console.WriteLine($"{sI.Id} - {sI.Name} - {sI.Quantity}"); break;
    case 'X': bool xB = TI.AddQuantity(TheIdRequest(), 1); Console.WriteLine(xB); break;
    case 'R': bool xR = await TI.RemoveQuantityAsync(TheIdRequest(),2); Console.WriteLine(xR); break;
    case 'Q': goto endNow;
        default: Console.WriteLine("=========================================="); break;
}

goto doAgain;


endNow: 
Console.WriteLine(" \n Bye");

string TheIdRequest()
{
    Console.WriteLine("Please enter id:");
    var x = Console.ReadLine();
    Console.WriteLine($"Searching for: {x} \n");
    return x?.Trim() ?? "349065925112023092242";
}