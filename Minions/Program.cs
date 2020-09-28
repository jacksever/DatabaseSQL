using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Minions
{
	class Program
	{
		static string connectionString = "Server=.;Database=Minions;Trusted_Connection=True";

		static void Main(string[] args)
		{
			SelectVillains();
			//InputData_3();
			//InputData_4();
			//InputData_5();
			//InputData_6();
		}

		static void InputData_3()
		{
			Console.Write("Введите ID злодея: ");
			int Id = Convert.ToInt32(Console.ReadLine());
			Console.WriteLine();

			SelectMinionsByVillain(Id);
		}

		static void InputData_4()
		{
			Console.Write("Minion: ");
			string dataMinion = Console.ReadLine();
			Console.Write("Villain: ");
			string dataVillain = Console.ReadLine();

			string[] words = dataMinion.Split(new char[] { ' ' });
			string nameMinion, townMinion;
			int ageMinion;

			if (words.Length < 3 || words.Length > 3)
			{
				Console.WriteLine("Неверно введённая информация. Повторите попытку..");
				return;
			}
			else
			{
				nameMinion = words[0];
				ageMinion = Convert.ToInt32(words[1]);
				townMinion = words[2];

				InsertMinionAndVillains(nameMinion, ageMinion, townMinion, dataVillain);
			}
		}

		static void InputData_5()
		{
			Console.Write("Введите ID злодея: ");
			int Id = Convert.ToInt32(Console.ReadLine());
			Console.WriteLine();

			DeleteVillain(Id);
		}

		static void InputData_6()
		{
			Console.Write("Ids: ");
			string dataMinion = Console.ReadLine();
			List<int> dataIds = new List<int>(dataMinion.Split(' ').Select(int.Parse).ToArray());

			if (dataIds.Count == 0)
			{
				Console.WriteLine("Неверно введённая информация. Повторите попытку..");
				return;
			}
			else
			{
				UpdateAgeMinions(dataIds);
			}
		}

		static void SelectVillains()
		{
			string selectionCommandString = $"SELECT Name, COUNT(*) AS Count " +
			$"FROM MinionsVillains AS MV JOIN Villains AS V ON V.Id = MV.VillianId " +
			$"GROUP BY Name " +
			$"HAVING COUNT(*) >= 3" +
			$"ORDER BY COUNT(*) DESC";
			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand command = new SqlCommand(selectionCommandString, connection);

			connection.Open();
			using (connection)
			{
				SqlDataReader reader = command.ExecuteReader();
				using (reader)
				{
					if (!reader.HasRows)
					{
						return;
					}
					while (reader.Read())
					{
						object name = reader["name"];
						object count = reader["count"];

						Console.WriteLine("{0} {1}", name, count);
					}
				}
			}
		}

		static void SelectMinionsByVillain(int id)
		{
			string selectNameVallains = $"SELECT Name FROM Villains WHERE Villains.Id = @id";
			string selectionCommandString = $"SELECT Name, Age FROM MinionsVillains " +
				$"JOIN Minions ON Minions.Id = MinionsVillains.MinionId WHERE MinionsVillains.VillianId = @id " +
				$"GROUP BY Name, Age ORDER BY Name";

			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand commandVillains = new SqlCommand(selectNameVallains, connection);
			SqlCommand commandMinions = new SqlCommand(selectionCommandString, connection);
			commandVillains.Parameters.AddWithValue("@id", id);
			commandMinions.Parameters.AddWithValue("@id", id);

			connection.Open();
			using (connection)
			{
				SqlDataReader readerVillains = commandVillains.ExecuteReader();
				using (readerVillains)
				{
					if (!readerVillains.HasRows)
					{
						Console.WriteLine("No villain with ID " + id + " exists in the database.");
						return;
					}

					while (readerVillains.Read())
					{
						object name = readerVillains["name"];
						Console.WriteLine("Vallain: {0}", name);
					}
				}

				SqlDataReader readerMinions = commandMinions.ExecuteReader();
				using (readerMinions)
				{
					if (!readerMinions.HasRows)
					{
						Console.WriteLine("(no minions)");
						return;
					}

					while (readerMinions.Read())
					{
						object name = readerMinions["name"];
						object age = readerMinions["age"];

						Console.WriteLine(" {0} {1}", name, age);
					}
				}
			}
		}

		static void InsertMinionAndVillains(string nameMinion, int ageMinion, string townMinion, string nameVillain)
		{
			SqlConnection connection = new SqlConnection(connectionString);

			string checkNameMinions = $"SELECT Name FROM Minions WHERE Minions.Name = @nameMinion";
			string checkNameVillain = $"SELECT Name FROM Villains WHERE Villains.Name = @nameVillain";
			string checkNameTown = $"SELECT Name FROM Towns WHERE Towns.Name = @townMinion";

			SqlCommand commandCheckMinion = new SqlCommand(checkNameMinions, connection);
			SqlCommand commandCheckVillain = new SqlCommand(checkNameVillain, connection);
			SqlCommand commandCheckTown = new SqlCommand(checkNameTown, connection);

			commandCheckMinion.Parameters.AddWithValue("@nameMinion", nameMinion);
			commandCheckVillain.Parameters.AddWithValue("@nameVillain", nameVillain);
			commandCheckTown.Parameters.AddWithValue("@townMinion", townMinion);

			connection.Open();
			using (connection)
			{
				SqlDataReader readerVillains = commandCheckVillain.ExecuteReader();
				using (readerVillains)
				{
					if (!readerVillains.HasRows)
					{
						SqlCommand insertVillain = new SqlCommand(
							$"INSERT INTO Villains " +
							$"(Name, EvilnessFactorId) VALUES " +
							$"(@name, 4)", connection);

						insertVillain.Parameters.AddWithValue("@name", nameVillain);

						readerVillains.Close();
						insertVillain.ExecuteNonQuery();

						Console.WriteLine("Злодей " + nameVillain + " был добавлен в базу данных.");
					}
				}

				SqlDataReader readerTowns = commandCheckTown.ExecuteReader();
				using (readerTowns)
				{
					if (!readerTowns.HasRows)
					{
						SqlCommand insertTown = new SqlCommand(
							$"INSERT INTO Towns " +
							$"(Name, CountryCode) VALUES " +
							$"(@name, 1)", connection);

						insertTown.Parameters.AddWithValue("@name", townMinion);

						readerTowns.Close();
						insertTown.ExecuteNonQuery();

						Console.WriteLine("Город " + townMinion + " был добавлен в базу данных.");
					}
				}

				SqlDataReader readerMinions = commandCheckMinion.ExecuteReader();
				using (readerMinions)
				{
					if (!readerMinions.HasRows)
					{
						SqlCommand insertMinion = new SqlCommand(
						$"INSERT INTO Minions " +
						$"(Name, Age, TownId) VALUES " +
						$"(@name, @age, (SELECT Id FROM Towns WHERE Name = @townId))", connection);

						insertMinion.Parameters.AddWithValue("@name", nameMinion);
						insertMinion.Parameters.AddWithValue("@age", ageMinion);
						insertMinion.Parameters.AddWithValue("@townId", townMinion);

						readerMinions.Close();
						insertMinion.ExecuteNonQuery();
					}
					else
					{
						Console.WriteLine("Данный миньон уже существует. Повторите попытку..");
						return;
					}
				}

				try
				{
					SqlCommand insertMinionVillains = new SqlCommand(
					$"INSERT INTO MinionsVillains " +
					$"(MinionId, VillianId) VALUES " +
					$"((SELECT Id FROM Minions WHERE Name = @minionName), (SELECT Id FROM Villains WHERE Name = @villianName))", connection);

					insertMinionVillains.Parameters.AddWithValue("@minionName", nameMinion);
					insertMinionVillains.Parameters.AddWithValue("@villianName", nameVillain);

					insertMinionVillains.ExecuteNonQuery();

					Console.WriteLine("Успешно добавлен " + nameMinion + ", чтобы быть миньоном " + nameVillain + ".");
				}
				catch
				{
					Console.WriteLine("Что-то пошло не так. Повторите попытку..");
				}
			}
		}

		static void DeleteVillain(int id)
		{
			SqlConnection connection = new SqlConnection(connectionString);

			string checkVillain = $"SELECT Name FROM Villains WHERE Id = @id";
			string countMinions = $"SELECT COUNT(*) AS Count FROM MinionsVillains WHERE VillianId = @id";

			SqlCommand commandCheckVillain = new SqlCommand(checkVillain, connection);
			SqlCommand commandCountMinions = new SqlCommand(countMinions, connection);
			commandCheckVillain.Parameters.AddWithValue("@id", id);
			commandCountMinions.Parameters.AddWithValue("@id", id);

			object name = null;
			object count = 0;

			connection.Open();
			using (connection)
			{
				SqlDataReader readerMinions = commandCountMinions.ExecuteReader();
				using (readerMinions)
				{
					if (readerMinions.HasRows)
					{
						while (readerMinions.Read())
						{
							count = readerMinions["count"];
						}

						try
						{
							SqlCommand deleteVillains = new SqlCommand(
							$"DELETE FROM MinionsVillains WHERE VillianId = @id", connection);

							deleteVillains.Parameters.AddWithValue("@id", id);

							readerMinions.Close();
							deleteVillains.ExecuteNonQuery();
						}
						catch (Exception e)
						{
							Console.WriteLine("Что-то пошло не так. Повторите попытку..");
						}
					}
				}

				SqlDataReader readerVillains = commandCheckVillain.ExecuteReader();
				using (readerVillains)
				{
					if (!readerVillains.HasRows)
					{
						Console.WriteLine("Такой злодей не найден.");
						return;
					}
					else
					{
						while (readerVillains.Read())
						{
							name = readerVillains["name"];
						}

						SqlCommand deleteVillains = new SqlCommand(
						$"DELETE FROM Villains WHERE Id = @id", connection);

						deleteVillains.Parameters.AddWithValue("@id", id);

						readerVillains.Close();
						deleteVillains.ExecuteNonQuery();

						Console.WriteLine("{0} был удален.", name);
						Console.WriteLine("{0} миньонов было освобождено.", count);
					}
				}
			}
		}

		static void UpdateAgeMinions(List<int> minionsIds)
		{
			var ids = string.Join(", ", minionsIds);

			SqlConnection connection = new SqlConnection(connectionString);

			connection.Open();
			using (connection)
			{
				try
				{
					string checkVillain = $"UPDATE Minions SET Age = Age + 1 WHERE Id IN ({ids})";

					SqlCommand commandCheckVillain = new SqlCommand(checkVillain, connection);
					commandCheckVillain.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					Console.WriteLine("Что-то пошло не так. Повторите попытку..");
					return;
				}

				string selectMinions = $"SELECT Name, Age FROM Minions";
				SqlCommand commandSelectMinion = new SqlCommand(selectMinions, connection);

				SqlDataReader readerMinions = commandSelectMinion.ExecuteReader();
				using (readerMinions)
				{
					while (readerMinions.Read())
					{
						object name = readerMinions["name"];
						object age = readerMinions["age"];

						Console.WriteLine(" {0} {1}", name, age);
					}
				}
			}
		}
	}
}
