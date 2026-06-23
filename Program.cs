using graph_tp.Models;
using graph_tp.Algorithms;
using graph_tp.Utils;
using System.Text;

namespace graph_tp;

public enum AlgorithmType
{
	BellmanFord = 1,
	FordFulkerson = 2,
	Prim = 3,
	WelshPowell = 4,
	Euleriano = 5,
	Hamiltoniano = 6
}

public class AlgorithmParameters
{
	public int? SourceVertex { get; set; }
	public int? TargetVertex { get; set; }
}

class Program
{
	private static Graph? _currentGraph;
	private static string? _currentFileName;
	private static QueryLogger _logger = new QueryLogger();

	static void Main(string[] args)
	{
		Console.Clear();
		PrintWelcomeBanner();
		_logger.StartSession();
		_logger.LogUserAction("Programa iniciado");

		try
		{
			while (true)
			{
				PrintMenu();

				Console.Write("\nEscolha uma opção: ");
				string? choice = Console.ReadLine();
				_logger.LogUserAction($"Opção escolhida: {choice ?? string.Empty}");

				Console.WriteLine();

				switch (choice)
				{
					case "1":
						_logger.LogSystemAction("Opção 1 selecionada: carregar grafo");
						LoadGraphFromFile();
						break;
					case "2":
						_logger.LogSystemAction("Opção 2 selecionada: Bellman-Ford");
						ExecuteAlgorithm(AlgorithmType.BellmanFord);
						break;
					case "3":
						_logger.LogSystemAction("Opção 3 selecionada: Ford-Fulkerson");
						ExecuteAlgorithm(AlgorithmType.FordFulkerson);
						break;
					case "4":
						_logger.LogSystemAction("Opção 4 selecionada: Prim");
						ExecuteAlgorithm(AlgorithmType.Prim);
						break;
					case "5":
						_logger.LogSystemAction("Opção 5 selecionada: Welsh-Powell");
						ExecuteAlgorithm(AlgorithmType.WelshPowell);
						break;
					case "6":
						_logger.LogSystemAction("Opção 6 selecionada: Euleriano");
						ExecuteAlgorithm(AlgorithmType.Euleriano);
						break;
					case "7":
						_logger.LogSystemAction("Opção 7 selecionada: Hamiltoniano");
						ExecuteAlgorithm(AlgorithmType.Hamiltoniano);
						break;
					case "8":
						_logger.LogUserAction("Usuário encerrou a aplicação");
						Console.WriteLine("Encerrando o sistema...");
						_logger.Dispose();
						return;
					default:
						OutputFormatter.PrintError("Opção inválida. Tente novamente.");
						_logger.LogError($"Opção inválida informada: {choice ?? string.Empty}");
						break;
				}

				if (choice != "8")
				{
					Console.WriteLine("\nPressione qualquer tecla para continuar...");
					Console.ReadKey();
					Console.Clear();
				}
			}
		}
		finally
		{
			_logger.Dispose();
		}
	}

	static void PrintWelcomeBanner()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║          ENTREGA MÁXIMA LOGÍSTICA S.A.                            ║
║          Sistema de Otimização de Malha de Distribuição           ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
        ");
		Console.ResetColor();
	}

	static void PrintMenu()
	{
		Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
		Console.WriteLine("║                    MENU PRINCIPAL                         ║");
		Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");

		if (_currentGraph != null)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"║  Grafo Carregado: {_currentFileName,-39} ║");
			Console.WriteLine($"║  Hubs: {_currentGraph.VertexCount,-3}  Rotas: {_currentGraph.EdgesCount,-3}                                  ║");
			Console.ResetColor();
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
		}

		Console.WriteLine("║                                                           ║");
		Console.WriteLine("║  1. Carregar Grafo (DIMACS)                               ║");
		Console.WriteLine("║                                                           ║");
		Console.WriteLine("║  ALGORITMOS:                                              ║");
		Console.WriteLine("║  2. Bellman-Ford                                           ║");
		Console.WriteLine("║  3. Ford-Fulkerson                                         ║");
		Console.WriteLine("║  4. Prim                                                   ║");
		Console.WriteLine("║  5. Welsh-Powell                                           ║");
		Console.WriteLine("║  6. Euleriano (Fleury)                                     ║");
		Console.WriteLine("║  7. Hamiltoniano                                           ║");
		Console.WriteLine("║                                                           ║");
		Console.WriteLine("║  8. Sair                                                   ║");
		Console.WriteLine("║                                                           ║");
		Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
	}

	static void LoadGraphFromFile()
	{
		var dataDirectory = LocateDataDirectory();
		if (dataDirectory == null)
		{
			OutputFormatter.PrintError("Não foi possível localizar a pasta Data a partir do diretório de execução.");
			_logger.LogError("Pasta Data não encontrada a partir do diretório de execução");
			return;
		}

		var dimacsFiles = Directory.GetFiles(dataDirectory, "*.dimacs")
			.OrderBy(file => Path.GetFileName(file))
			.ToList();

		if (dimacsFiles.Count == 0)
		{
			OutputFormatter.PrintError("Nenhum arquivo DIMACS encontrado na pasta Data.");
			_logger.LogError("Nenhum arquivo DIMACS encontrado na pasta Data");
			return;
		}

		Console.WriteLine("Arquivos DIMACS disponíveis em Data:");
		for (int i = 0; i < dimacsFiles.Count; i++)
		{
			Console.WriteLine($"  {i + 1}. {Path.GetFileName(dimacsFiles[i])}");
		}

		Console.Write("Escolha o arquivo para carregar: ");
		string? selectedIndex = Console.ReadLine();
		_logger.LogUserAction($"Arquivo DIMACS escolhido: {selectedIndex ?? string.Empty}");

		if (!int.TryParse(selectedIndex, out int fileChoice) || fileChoice < 1 || fileChoice > dimacsFiles.Count)
		{
			OutputFormatter.PrintError("Escolha inválida.");
			_logger.LogError("Escolha inválida na seleção do arquivo DIMACS");
			return;
		}

		string filePath = dimacsFiles[fileChoice - 1];

		try
		{
			_currentGraph = DimacsParser.LoadFromFile(filePath);
			_currentFileName = Path.GetFileName(filePath);

			_logger.LogUserAction($"Grafo carregado com sucesso: {filePath}");
			_logger.LogGraphInfo(_currentGraph.VertexCount, _currentGraph.EdgesCount);

			OutputFormatter.PrintSuccess($"Grafo carregado com sucesso!");
			OutputFormatter.PrintGraphInfo(_currentGraph);
			
			if (_logger.IsActive)
			{
				OutputFormatter.PrintInfo($"Log ativo: logs/{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMdd}.log");
			}
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro ao carregar arquivo: {ex.Message}");
			_logger.LogError($"Erro ao carregar arquivo: {ex.Message}");
			_currentGraph = null;
			_currentFileName = null;
		}
	}

	static string? LocateDataDirectory()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);

		while (directory != null)
		{
			var candidate = Path.Combine(directory.FullName, "Data");
			if (Directory.Exists(candidate))
			{
				return candidate;
			}

			directory = directory.Parent;
		}

		return null;
	}

	static void ExecuteAlgorithm(AlgorithmType algorithm)
	{
		if (!CheckGraphLoaded()) return;

		try
		{
			switch (algorithm)
			{
				case AlgorithmType.BellmanFord:
					ExecuteBellmanFord();
					break;
				case AlgorithmType.FordFulkerson:
					ExecuteFordFulkerson();
					break;
				case AlgorithmType.Prim:
					ExecutePrim();
					break;
				case AlgorithmType.WelshPowell:
					ExecuteWelshPowell();
					break;
				case AlgorithmType.Euleriano:
					ExecuteEuleriano();
					break;
				case AlgorithmType.Hamiltoniano:
					ExecuteHamiltoniano();
					break;
			}
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro ao executar algoritmo: {ex.Message}");
			_logger.LogError($"Erro ao executar {algorithm}: {ex.Message}");
		}
	}

	static int RequestSourceVertex()
	{
		Console.WriteLine();
		Console.WriteLine("=== Parâmetros do Algoritmo ===");
		Console.Write("Digite o vértice de origem/fonte (1-{0}): ", _currentGraph!.VertexCount);

		if (!int.TryParse(Console.ReadLine(), out int source) || !_currentGraph.Containsvertex(source))
		{
			OutputFormatter.PrintError("Vértice de origem inválido.");
			_logger.LogError("Vértice de origem inválido fornecido pelo usuário");
			throw new InvalidOperationException("Vértice de origem inválido.");
		}

		_logger.LogUserAction($"Vértice de origem configurado: {source}");
		return source;
	}

	static int RequestTargetVertex()
	{
		Console.WriteLine();
		Console.WriteLine("=== Parâmetros do Algoritmo ===");
		Console.Write("Digite o vértice de destino/sumidouro (1-{0}): ", _currentGraph!.VertexCount);

		if (!int.TryParse(Console.ReadLine(), out int target) || !_currentGraph.Containsvertex(target))
		{
			OutputFormatter.PrintError("Vértice de destino inválido.");
			_logger.LogError("Vértice de destino inválido fornecido pelo usuário");
			throw new InvalidOperationException("Vértice de destino inválido.");
		}

		_logger.LogUserAction($"Vértice de destino configurado: {target}");
		return target;
	}

	static void ExecuteBellmanFord()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Bellman-Ford ===");
		_logger.LogSystemAction("Iniciando Bellman-Ford");

		try
		{
			int source = RequestSourceVertex();
			var result = BellmanFordAlgorithm.RunBellmanFord(_currentGraph!, source, _logger);

			Console.WriteLine();
			BellmanFordAlgorithm.PrintResult(result);

			_logger.LogUserAction("Bellman-Ford executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Bellman-Ford: {ex.Message}");
			_logger.LogError($"Erro em Bellman-Ford: {ex.Message}");
		}
	}

	static void ExecuteFordFulkerson()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Ford-Fulkerson ===");
		_logger.LogSystemAction("Iniciando Ford-Fulkerson");

		try
		{
			int source = RequestSourceVertex();
			int target = RequestTargetVertex();

			var result = FordFulkersonAlgorithm.FordFulkerson(
				_currentGraph!,
				source,
				target,
				_logger);

			Console.WriteLine();
			FordFulkersonAlgorithm.PrintResult(result);

			_logger.LogUserAction("Ford-Fulkerson executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Ford-Fulkerson: {ex.Message}");
			_logger.LogError($"Erro em Ford-Fulkerson: {ex.Message}");
		}
	}

	static void ExecutePrim()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Prim ===");
		_logger.LogSystemAction("Iniciando Prim");

		try
		{
			int root = RequestSourceVertex();
			var result = PrimAlgorithm.RunPrim(_currentGraph!, root, _logger);

			Console.WriteLine();
			PrimAlgorithm.PrintResult(result);

			_logger.LogUserAction("Prim executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Prim: {ex.Message}");
			_logger.LogError($"Erro em Prim: {ex.Message}");
		}
	}

	static void ExecuteWelshPowell()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Welsh-Powell ===");
		_logger.LogSystemAction("Iniciando Welsh-Powell");

		try
		{
			var result = WelshPowellAlgorithm.RunWelshPowell(_currentGraph!, _logger);

			Console.WriteLine();
			WelshPowellAlgorithm.PrintResult(result);

			_logger.LogUserAction("Welsh-Powell executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Welsh-Powell: {ex.Message}");
			_logger.LogError($"Erro em Welsh-Powell: {ex.Message}");
		}
	}

	static void ExecuteEuleriano()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Euleriano (Fleury) ===");
		_logger.LogSystemAction("Iniciando Euleriano (Fleury)");

		try
		{
			var result = FleuryAlgorithm.RunFleury(_currentGraph!, _logger);

			Console.WriteLine();
			FleuryAlgorithm.PrintResult(result);

			_logger.LogUserAction("Euleriano (Fleury) executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Euleriano (Fleury): {ex.Message}");
			_logger.LogError($"Erro em Euleriano (Fleury): {ex.Message}");
		}
	}

	static void ExecuteHamiltoniano()
	{
		Console.WriteLine();
		Console.WriteLine("=== Executando Hamiltoniano ===");
		_logger.LogSystemAction("Iniciando Hamiltoniano");

		try
		{
			var result = HamiltonAlgorithm.RunHamiltonian(_currentGraph!, _logger);

			Console.WriteLine();
			HamiltonAlgorithm.PrintResult(result);

			_logger.LogUserAction("Hamiltoniano executado com sucesso");
		}
		catch (Exception ex)
		{
			OutputFormatter.PrintError($"Erro em Hamiltoniano: {ex.Message}");
			_logger.LogError($"Erro em Hamiltoniano: {ex.Message}");
		}
	}

	static bool CheckGraphLoaded()
	{
		if (_currentGraph == null)
		{
			OutputFormatter.PrintError("Nenhum grafo carregado. Por favor, carregue um arquivo DIMACS primeiro (opção 1).");
			_logger.LogError("Tentativa de executar algoritmo sem grafo carregado");
			return false;
		}
		return true;
	}
}