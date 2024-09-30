using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
class User
{
    public int UserID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public void HashPassword()
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            Password = builder.ToString();
        }
    }
}

class Wallet
{
    public int WalletID { get; set; }
    public int UserID { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
}

class UserManager
{
    private List<User> users = new List<User>();
    private int activeUserID = -1;
    private List<Wallet> wallets = new List<Wallet>();
    private int activeWalletID = -1;

public UserManager()  // init func
    {
        // Initialize the lists
        users = LoadUsersFromStorage();
        wallets = LoadWalletsFromStorage();
    }

    // Existing methods (Register, Login, Logout, etc.)

private List<User> LoadUsersFromStorage()
    {
        List<User> loadedUsers = new List<User>();

        try
        {
            // Read user data from storage file and populate the list
            string[] userLines = File.ReadAllLines("storageuser.txt");

            foreach (string line in userLines)
            {
                string[] parts = line.Split(',');
                User loadedUser = new User
                {
                    UserID = Convert.ToInt32(parts[0]),
                    Name = parts[1],
                    Email = parts[2],
                    Password = parts[3]
                };
                loadedUsers.Add(loadedUser);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Storage file for users not found. Creating a new one.");
        }

        return loadedUsers;
    }

private List<Wallet> LoadWalletsFromStorage()
    {
        List<Wallet> loadedWallets = new List<Wallet>();

        try
        {
            // Read wallet data from storage file and populate the list
            string[] walletLines = File.ReadAllLines("storagewallets.txt");

            foreach (string line in walletLines)
            {
                string[] parts = line.Split(',');
                Wallet loadedWallet = new Wallet
                {
                    WalletID = Convert.ToInt32(parts[0]),
                    UserID = Convert.ToInt32(parts[1]),
                    Balance = Convert.ToDecimal(parts[2]),
                    Currency = parts[3]
                };
                loadedWallets.Add(loadedWallet);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Storage file for wallets not found. Creating a new one.");
        }

        return loadedWallets;
    }


public void Register()
    {
        Console.WriteLine("Enter name:");
        string name = Console.ReadLine();
        Console.WriteLine("Enter email:");
        string email = Console.ReadLine();
        Console.WriteLine("Enter password:");
        string password = Console.ReadLine();

        User newUser = new User
        {
            UserID = users.Count + 1,
            Name = name,
            Email = email,
            Password = password
        };
        newUser.HashPassword();

        users.Add(newUser);
        SaveUserDetails(newUser);
    }

public void Login()
{
    if (activeUserID != -1)
    {
        Console.WriteLine("You are already logged in. Please logout to login as another user.");
        return;
    }

    Console.WriteLine("Enter email:");
    string email = Console.ReadLine();
    Console.WriteLine("Enter password:");
    string password = Console.ReadLine();

    foreach (User user in users)
    {
        if (user.Email == email)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                string hashedPassword = builder.ToString();

                if (user.Password == hashedPassword)
                {
                    activeUserID = user.UserID;
                    Console.WriteLine("Login successful!");
                    return;
                }
            }
        }
    }

    Console.WriteLine("Invalid email or password.");
}


public void Logout()
    {
        activeUserID = -1;
        Console.WriteLine("Logged out successfully.");
    }

public void CreateWallet()
    {
        if (activeUserID == -1)
        {
            Console.WriteLine("Please login first.");
            return;
        }

        Console.WriteLine("Enter starting balance:");
        decimal startingBalance = Convert.ToDecimal(Console.ReadLine());
        Console.WriteLine("Enter currency:");
        string currency = Console.ReadLine();

        Wallet newWallet = new Wallet
        {
            WalletID = wallets.Count + 1,
            UserID = activeUserID,
            Balance = startingBalance,
            Currency = currency
        };

        wallets.Add(newWallet);
        SaveWalletDetails(newWallet);
        Console.WriteLine("Wallet created successfully with ID !" + newWallet.WalletID);
    }

public void ChooseWallet()
    {
        if (activeUserID == -1)
        {
            Console.WriteLine("Please login first.");
            return;
        }

        Console.WriteLine("Enter wallet ID:");
        int walletID = Convert.ToInt32(Console.ReadLine());

        foreach (Wallet wallet in wallets)
        {
            if (wallet.UserID == activeUserID && wallet.WalletID == walletID)
            {
                activeWalletID = walletID;
                Console.WriteLine("Wallet selected successfully!");
                return;
            }
        }

        Console.WriteLine("Wallet not found or doesn't belong to the active user.");
    }

public void DeleteWallet()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    Console.WriteLine("Enter wallet ID to delete:");
    int walletID = Convert.ToInt32(Console.ReadLine());

    for (int i = 0; i < wallets.Count; i++)
    {
        if (wallets[i].UserID == activeUserID && wallets[i].WalletID == walletID)
        {
            // Remove wallet from the in-memory list
            Wallet deletedWallet = wallets[i];
            wallets.RemoveAt(i);
            Console.WriteLine("Wallet deleted successfully!");

            // Remove wallet from the storage file
            UpdateWalletStorageFile(deletedWallet);
            return;
        }
    }

    Console.WriteLine("Wallet not found or doesn't belong to the active user.");
}

public void Addopmanager(){
Console.WriteLine("1 -Income\n2- Expense\n");
int choice = Convert.ToInt32(Console.ReadLine());
switch (choice){
        case 1:
            AddIncome();
            break;
        case 2:
            AddExpense();
            break;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;

}

}
public void AddIncome()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    if (activeWalletID == -1)
    {
        Console.WriteLine("Please choose a wallet first.");
        return;
    }

    Console.WriteLine("Enter the amount of income:");
    decimal incomeAmount = Convert.ToDecimal(Console.ReadLine());
    Console.WriteLine("write Category: Salary/scholarhip/etc..");
    string Category = Console.ReadLine();
    Wallet activeWallet = wallets.Find(wallet => wallet.WalletID == activeWalletID); // Retrieve the active wallet using activeWalletID from the wallets list
    decimal currentBalance = activeWallet.Balance;
    activeWallet.Balance = currentBalance + incomeAmount;

    Console.WriteLine("Income added successfully!");

    // Save the updated wallet balance to storage
    UpdateWalletDetails(activeWallet);

    // Add the operation to the history
    DateTime operationDateTime = DateTime.Now;
    AddOperationToHistory(activeUserID, activeWalletID, operationDateTime, "Income", incomeAmount, currentBalance, activeWallet.Balance, Category);
}

public void AddExpense()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    if (activeWalletID == -1)
    {
        Console.WriteLine("Please choose a wallet first.");
        return;
    }

    Console.WriteLine("Enter the amount of expense:");
    decimal expenseAmount = Convert.ToDecimal(Console.ReadLine());

    Wallet activeWallet = wallets.Find(wallet => wallet.WalletID == activeWalletID); // Retrieve the active wallet using activeWalletID from the wallets list

    if (activeWallet.Balance < expenseAmount)
    {
        Console.WriteLine("Insufficient balance in the wallet.");
        return;
    }

    Console.WriteLine("write Category: Food/Fees/Car/Debts/etc..");
    string Category = Console.ReadLine();
    decimal currentBalance = activeWallet.Balance;
    activeWallet.Balance = currentBalance - expenseAmount;

    Console.WriteLine("Expense added successfully!");

    // Save the updated wallet balance to storage
    UpdateWalletDetails(activeWallet);

    // Add the operation to the history
    DateTime operationDateTime = DateTime.Now;
    AddOperationToHistory(activeUserID, activeWalletID, operationDateTime, "Expense", expenseAmount, currentBalance, activeWallet.Balance,Category);
}

public void CalculateStatistics()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    if (activeWalletID == -1)
    {
        Console.WriteLine("Please choose a wallet first.");
        return;
    }

    Console.WriteLine("Enter the start date (dd-MM-yyyy):");
    string startDateInput = Console.ReadLine();

    Console.WriteLine("Enter the end date (dd-MM-yyyy):");
    string endDateInput = Console.ReadLine();

    if (!DateTime.TryParseExact(startDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
    {
        Console.WriteLine("Invalid start date format.");
        return;
    }

    if (!DateTime.TryParseExact(endDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
    {
        Console.WriteLine("Invalid end date format.");
        return;
    }

    if (endDate < startDate)
    {
        Console.WriteLine("End date cannot be earlier than the start date.");
        return;
    }

    decimal totalIncome = 0;
    decimal totalExpense = 0;
    decimal initialbalance = 0;
    decimal finalWalletBalance = 0;
    bool flagofstart = false;
    // Iterate over the operation history file to calculate the statistics
    string[] operationLogs = File.ReadAllLines("operationhistory.txt");
    foreach (string operationLog in operationLogs)
    {
        string[] parts = operationLog.Split(',');

        int userID = Convert.ToInt32(parts[0]);
        int walletID = Convert.ToInt32(parts[1]);
        DateTime operationDateTime = DateTime.ParseExact(parts[2].Replace("Date/Time: ", "").Trim(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
        string operationType = parts[3];
        decimal amount = Convert.ToDecimal(parts[4]);
        decimal currentBalance = Convert.ToDecimal(parts[5]);
        decimal finalBalance = Convert.ToDecimal(parts[6]);
        string category = parts[7];

        // Check if the operation belongs to the active user and falls within the specified date range
        if (userID == activeUserID && walletID == activeWalletID && operationDateTime.Date >= startDate.Date && operationDateTime.Date <= endDate.Date)
        {
            if (operationType == "Income")
            {
                totalIncome += amount;
            }
            else if (operationType == "Expense")
            {
                totalExpense += amount;
            }

            if (operationDateTime.Date >= startDate.Date && flagofstart == false)
            {
                initialbalance = currentBalance;
                flagofstart = true;
            }

            if (operationDateTime.Date <= endDate.Date)
            {
                finalWalletBalance = finalBalance;
            }
        }
    }

    decimal overallBalanceChange = finalWalletBalance - initialbalance ;
    decimal changepercentage = (overallBalanceChange / initialbalance) * 100 ;

    Console.WriteLine($"Income amount between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}: {totalIncome}");
    Console.WriteLine($"Expense amount between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}: {totalExpense}");
    Console.WriteLine($"Overall balance change between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}: {overallBalanceChange}");
     Console.WriteLine($"Overall balance Percentage change between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}: {changepercentage} %");
}


private void UpdateWalletStorageFile(Wallet deletedWallet)
{
    // Read all lines from the storagewallets.txt file
    string[] lines = File.ReadAllLines("storagewallets.txt");

    // Remove the line that matches the deleted wallet's information
    List<string> updatedLines = new List<string>();
    foreach (string line in lines)
    {
        string[] parts = line.Split(',');
        int walletID = Convert.ToInt32(parts[0]);
        int userID = Convert.ToInt32(parts[1]);

        if (walletID != deletedWallet.WalletID || userID != deletedWallet.UserID)
        {
            updatedLines.Add(line);
        }
    }

    // Write back the updated lines to the storagewallets.txt file
    File.WriteAllLines("storagewallets.txt", updatedLines);
}

private void UpdateWalletDetails(Wallet updatedWallet)
{
    int index = wallets.FindIndex(wallet => wallet.WalletID == updatedWallet.WalletID);

    if (index != -1)
    {
        // Wallet with the same ID exists in the list, update it
        wallets[index] = updatedWallet;

        // Save the updated list of wallets to storage
        SaveWalletsToStorage(wallets);

        Console.WriteLine("Wallet details updated and saved successfully!");
    }
    else
    {
        Console.WriteLine("Wallet not found. Unable to update and save wallet details.");
    }
}

private void SaveUserDetails(User user)
    {
        // Code to save user details to storageuser.txt
        using (StreamWriter writer = new StreamWriter("storageuser.txt", true))
        {
            writer.WriteLine($"{user.UserID},{user.Name},{user.Email},{user.Password}");
        }
    }

private void SaveWalletDetails(Wallet wallet)
    {
        // Code to save wallet details to storagewallets.txt
        using (StreamWriter writer = new StreamWriter("storagewallets.txt", true))
        {
            writer.WriteLine($"{wallet.WalletID},{wallet.UserID},{wallet.Balance},{wallet.Currency}");
        }
    }
private void SaveWalletsToStorage(List<Wallet> wallets)
{
    using (StreamWriter writer = new StreamWriter("storagewallets.txt"))
    {
        foreach (Wallet wallet in wallets)
        {
            string walletLine = $"{wallet.WalletID},{wallet.UserID},{wallet.Balance},{wallet.Currency}";
            writer.WriteLine(walletLine);
        }
    }
}

private void AddOperationToHistory(int userID, int walletID, DateTime dateTime, string operationType, decimal amount, decimal currentBalance, decimal finalBalance, string category)
{
    string operationLog = $"{userID},{walletID},{dateTime},{operationType},{amount},{currentBalance},{finalBalance},{category}";

    // Append the operation log to the operation history file
    File.AppendAllText("operationhistory.txt", operationLog + Environment.NewLine);
}


public void ListOperationLog()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    string[] operationLogs = File.ReadAllLines("operationhistory.txt");
    foreach (string operationLog in operationLogs)
    {
        string[] parts = operationLog.Split(',');

        int userID = Convert.ToInt32(parts[0]);
        int walletID = Convert.ToInt32(parts[1]);
        DateTime operationDateTime = Convert.ToDateTime(parts[2]);
        string operationType = parts[3];
        decimal amount = Convert.ToDecimal(parts[4]);
        decimal currentBalance = Convert.ToDecimal(parts[5]);
        decimal finalBalance = Convert.ToDecimal(parts[6]);
        string category = parts[7];

        if (userID == activeUserID)
        {
            Console.WriteLine($"Date/Time: {operationDateTime}");
            Console.WriteLine($"walletID: {walletID}");
            Console.WriteLine($"Operation Type: {operationType}");
            Console.WriteLine($"Amount: {amount}");
            Console.WriteLine($"Current Balance: {currentBalance}");
            Console.WriteLine($"Final Balance: {finalBalance}");
            Console.WriteLine($"Category: {category}");
            Console.WriteLine("\n");
        }
    }
}

public void ListUserWallets()
{
    if (activeUserID == -1)
    {
        Console.WriteLine("Please login first.");
        return;
    }

    List<Wallet> userWallets = wallets.FindAll(wallet => wallet.UserID == activeUserID);

    Console.WriteLine("User Wallets:");
    foreach (Wallet wallet in userWallets)
    {
        Console.WriteLine($"Wallet ID: {wallet.WalletID}");
        Console.WriteLine($"Balance: {wallet.Balance}");
        Console.WriteLine($"Currency: {wallet.Currency}");
        Console.WriteLine("\n");
    }
}

public void PrintActiveUserDetails()
{
    int LoggedInIndex = users.FindIndex(user => user.UserID == activeUserID);
    if (LoggedInIndex != -1)
    {
        // Retrieve the user details from the users list based on the activeUserID
        User activeUser = users[LoggedInIndex];

        Console.WriteLine($"Logged in: user id: {activeUser.UserID}, email: {activeUser.Email}");
    }
    else
    {
        Console.WriteLine("Logged in: False");
    }
}
}

class Program
{

    static void Main(string[] args)
    {
        UserManager userManager = new UserManager();
        bool exit = false;
        while (!exit)
        {
            userManager.PrintActiveUserDetails();
            Console.WriteLine("1 - Register\n2 - Login\n3 - Logout\n4 - Create Wallet\n5 - Choose Wallet\n6 - Delete Wallet\n7 - Add Operation\n8 - Check Statistics\n9 - List wallets\n10- List Op logs\n11 - Exit");
            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    userManager.Register();
                    break;
                case 2:
                    userManager.Login();
                    break;
                case 3:
                    userManager.Logout();
                    break;
                case 4:
                    userManager.CreateWallet();
                    break;
                case 5:
                    userManager.ChooseWallet();
                    break;
                case 6:
                    userManager.DeleteWallet();
                    break;
                case 7:
                    userManager.Addopmanager();
                    break;
                case 8:
                    userManager.CalculateStatistics();
                    break;
                // Implement other cases for AddOperation, CheckStatistics, etc.
                case 9:
                    userManager.ListUserWallets();
                    break;
                case 10:
                    userManager.ListOperationLog();
                    break;
                case 11:
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}
