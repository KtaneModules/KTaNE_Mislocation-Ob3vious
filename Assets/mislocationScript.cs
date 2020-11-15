using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class mislocationScript : MonoBehaviour {

	//publics
	public KMAudio Audio;
	public KMSelectable[] Buttons;
	public MeshRenderer Statuslight;
	public KMBombModule Module;

	//system
	private bool solved;
	private int[][] grid = { new int[] { 1 } };
	private int[] coordinates = new int[2];
	private int groups = 9;
	private bool[] highlighted = new bool[6];
	private bool isanimating;
	private int[] optimal;

	//logging
	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler Press (int pos)
	{
		return delegate
		{
			Buttons[pos].AddInteractionPunch();
			Audio.PlaySoundAtTransform("Tap", Module.transform);
			if (!solved && !isanimating)
			{
				switch (pos)
				{
					case 0:
						if (coordinates[1] != 0 && grid[coordinates[1] - 1][coordinates[0]] != 0)
							coordinates[1]--;
						else
							return false;
						break;
					case 1:
						if (coordinates[1] != grid.Length - 1 && grid[coordinates[1] + 1][coordinates[0]] != 0)
							coordinates[1]++;
						else
							return false;
						break;
					case 2:
						if (coordinates[0] != 0 && grid[coordinates[1]][coordinates[0] - 1] != 0)
							coordinates[0]--;
						else
							return false;
						break;
					case 3:
						if (coordinates[0] != grid[coordinates[1]].Length - 1 && grid[coordinates[1]][coordinates[0] + 1] != 0)
							coordinates[0]++;
						else
							return false;
						break;
					case 4:
						int y = Rnd.Range(0, grid.Length);
						int x = Rnd.Range(0, grid[y].Length);
						while (grid[y][x] == 0)
						{
							y = Rnd.Range(0, grid.Length);
							x = Rnd.Range(0, grid[y].Length);
						}
						coordinates = new int[] { x, y };
						Debug.LogFormat("[Mislocation #{0}] {1}", _moduleID, coordinates.Select(z => z + 1).Join(", "));
						return false;
					case 5:
						if (grid[coordinates[1]][coordinates[0]] == groups + 1)
						{
							Module.HandlePass();
							Statuslight.material.color = new Color(1, 1, 1, 0.75f);
							for (int j = 0; j < 6; j++)
							{
								Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
							}
							solved = true;
						}
						else
						{
							Module.HandleStrike();
							StartCoroutine(Strike());
						}
						return false;
				}
				for (int i = 0; i < grid.Length; i++)
				{
					for (int j = 0; j < grid[i].Length; j++)
					{
						if (grid[i][j] == grid[coordinates[1]][coordinates[0]] && !(coordinates[0] == j && coordinates[1] == i))
						{
							coordinates = new int[] { j, i };
							Debug.LogFormat("[Mislocation #{0}] {1}", _moduleID, coordinates.Select(x => x + 1).Join(", "));
							return false;
						}
					}
				}
				Debug.LogFormat("[Mislocation #{0}] {1}", _moduleID, coordinates.Select(x => x + 1).Join(", "));
			}
			return false;
		};
	}

	void Awake ()
	{
		_moduleID = _moduleIdCounter++;
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].OnInteract += Press(i);
			int x = i;
			Buttons[i].OnHighlight += delegate { highlighted[x] = true; };
			Buttons[i].OnHighlightEnded += delegate { highlighted[x] = false; };
		}
	}

	void Start()
	{
		regenerate:
		Mazegen(groups, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", '*');
		if (!CheckSolve(groups * 2 + 1))
			goto regenerate;
	}

	void Update ()
	{
		if (!isanimating && !solved)
		{
			if (coordinates[1] != 0 && grid[coordinates[1] - 1][coordinates[0]] != 0 && !highlighted[0])
				Buttons[0].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			else
				Buttons[0].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			if (coordinates[1] != grid.Length - 1 && grid[coordinates[1] + 1][coordinates[0]] != 0 && !highlighted[1])
				Buttons[1].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			else
				Buttons[1].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			if (coordinates[0] != 0 && grid[coordinates[1]][coordinates[0] - 1] != 0 && !highlighted[2])
				Buttons[2].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			else
				Buttons[2].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			if (coordinates[0] != grid[coordinates[1]].Length - 1 && grid[coordinates[1]][coordinates[0] + 1] != 0 && !highlighted[3])
				Buttons[3].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			else
				Buttons[3].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			for (int i = 4; i < 6; i++)
			{
				if (!highlighted[i])
					Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
				else
					Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			}
		}
	}
	void Mazegen (int size, string chars, char special)
	{
		grid = new int[][] { new int[] { 1 } };
		while (grid.Select(z => z.Sum()).Sum() < size * 2 + 1)
		{
			int dir = Rnd.Range(0, 4);
			int y = Rnd.Range(0, grid.Length);
			int x = Rnd.Range(0, grid[y].Length);
			while (grid[y][x] == 0)
			{
				y = Rnd.Range(0, grid.Length);
				x = Rnd.Range(0, grid[y].Length);
			}
			switch (dir)
			{
				case 0:
					//up
					if (y == 0)
						grid = new int[][] { grid[0].Select(z => 0).ToArray() }.Concat(grid).ToArray();
					else
						y--;
					grid[y][x] = 1;
					break;
				case 1:
					//down
					if (y == grid.Length - 1)
						grid = grid.Concat(new int[][] { grid[0].Select(z => 0).ToArray() }).ToArray();
					y++;
					grid[y][x] = 1;
					break;
				case 2:
					//left
					if (x == 0)
						grid = grid.Select(z => new int[] { 0 }.Concat(z).ToArray()).ToArray();
					else
						x--;
					grid[y][x] = 1;
					break;
				case 3:
					//right
					if (x == grid[y].Length - 1)
						grid = grid.Select(z => z.Concat(new int[] { 0 }).ToArray()).ToArray();
					x++;
					grid[y][x] = 1;
					break;
			}
		}
		int[] reordered = Enumerable.Range(1, size).Concat(Enumerable.Range(1, size)).Concat(new int[] { size + 1 }).ToList().Shuffle().ToArray();
		int k = 0;
		for (int i = 0; i < grid.Length; i++)
		{
			for (int j = 0; j < grid[i].Length; j++)
			{
				if (grid[i][j] != 0)
				{
					grid[i][j] = reordered[k];
					k++;
				}
			}
		}
		Debug.LogFormat("[Mislocation #{0}] (use slashes as new line separators) /{1}", _moduleID, grid.Select(z => z.Select(w => ("-" + chars.Substring(0, size) + special.ToString())[w]).Join("")).Join("/").Replace("0", " "));
		{
			int y = Rnd.Range(0, grid.Length);
			int x = Rnd.Range(0, grid[y].Length);
			while (grid[y][x] == 0)
			{
				y = Rnd.Range(0, grid.Length);
				x = Rnd.Range(0, grid[y].Length);
			}
			coordinates = new int[] { x, y };
			Debug.LogFormat("[Mislocation #{0}] {1}", _moduleID, coordinates.Select(z => z + 1).Join(", "));
		}
	}

	bool CheckSolve(int tilecount)
	{
		int[,] matrix = new int[tilecount, tilecount];
		int n = 0;
		int p = 0;
		for (int i = 0; i < grid.Length; i++)
		{
			for (int j = 0; j < grid[i].Length; j++)
			{
				if (grid[i][j] != 0)
				{
					for (int k = 0; k < 4; k++)
					{
						int x = -1, y = -1;
						switch (k)
						{
							case 0:
								if (i > 0 && grid[i - 1][j] != 0)
								{
									x = j;
									y = i - 1;
								}
								break;
							case 1:
								if (i < grid.Length - 1 && grid[i + 1][j] != 0)
								{
									x = j;
									y = i + 1;
								}
								break;
							case 2:
								if (j > 0 && grid[i][j - 1] != 0)
								{
									x = j - 1;
									y = i;
								}
								break;
							case 3:
								if (j < grid[i].Length - 1 && grid[i][j + 1] != 0)
								{
									x = j + 1;
									y = i;
								}
								break;
						}
						if (x >= 0 && y >= 0)
						{
							bool yes = true;
							int o = 0;
							for (int l = 0; l < grid.Length && yes; l++)
							{
								for (int m = 0; m < grid[l].Length && yes; m++)
								{
									//Debug.Log(x + "," + y + ";" + m + "," + l);
									if (grid[y][x] == grid[l][m] && (x != m || y != l))
									{
										yes = false;
										x = m;
										y = l;
										matrix[o, n] = k + 1;
									}
									else if (grid[y][x] == grid[l][m] && grid[y][x] == tilecount / 2 + 1)
									{
										yes = false;
										matrix[o, n] = k + 1;
										p = o;
									}
									else if (grid[l][m] != 0)
										o++;
								}
							}
						}
					}
					n++;
				}
			}
		}
		//string hlep = string.Empty;
		//for (int i = 0; i < tilecount; i++)
		//{
		//	for (int j = 0; j < tilecount; j++)
		//	{
		//		hlep += matrix[i, j];
		//	}
		//	hlep += "\n";
		//}
		//Debug.Log(hlep);
		int[,] matrixbk = new int[tilecount, tilecount];
		bool check = true;
		while (check)
		{
			for (int i = 0; i < tilecount; i++)
				for (int j = 0; j < tilecount; j++)
					for (int k = 0; k < tilecount; k++)
						if (matrix[k, i] == 0 && matrix[j, i] != 0 && matrix[k, j] != 0)
							matrixbk[k, i] = matrix[j, i];
			check = false;
			for (int i = 0; i < tilecount; i++)
				for (int j = 0; j < tilecount; j++)
					if (matrix[i, j] == 0 && matrixbk[i, j] != 0)
					{
						check = true;
						matrix[i, j] = matrixbk[i, j];
					}
		}
		optimal = new int[tilecount];
		//string hlep = string.Empty;
		for (int i = 0; i < tilecount; i++)
			optimal[i] = matrix[p, i] - 1;
		bool goodtogo = true;
		for (int i = 0; i < tilecount; i++)
			for (int j = 0; j < tilecount; j++)
				if (matrix[i, j] == 0)
					goodtogo = false;
		if (!goodtogo)
			Debug.LogFormat("[Mislocation #{0}] The maze appears to not be strongly connected. Regenerating...", _moduleID);
		return goodtogo;
	}

	IEnumerator Strike()
	{
		isanimating = true;
		for (int j = 0; j < 6; j++)
			Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 6; j++)
				Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			yield return new WaitForSeconds(0.1f);
			for (int j = 0; j < 6; j++)
				Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			yield return new WaitForSeconds(0.1f);
		}
		isanimating = false;
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} submit' to submit the current position, '!{0} relocate' to randomise position. '!{0} uldr' to press those directions.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "submit")
			Buttons[5].OnInteract();
		else if (command == "relocate")
			Buttons[4].OnInteract();
		else
		{
			for (int i = 0; i < command.Length; i++)
			{
				if (!"udlr".Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			}
			for (int i = 0; i < command.Length; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if ("udlr"[j] == command[i])
					{
						Buttons[j].OnInteract();
						yield return null;
					}
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		while (grid[coordinates[1]][coordinates[0]] != groups + 1)
		{
			int z = 0;
			for (int i = 0; i < grid.Length; i++)
				for (int j = 0; j < grid[i].Length; j++)
				{
					if (j == coordinates[0] && i == coordinates[1])
					{
						Debug.Log(z + ", " + optimal[z]);
						Buttons[optimal[z]].OnInteract();
						yield return true;
						j = grid[i].Length;
						i = grid.Length - 1;
					}
					else if (grid[i][j] != 0)
						z++;
				}
		}
		Buttons[5].OnInteract();
	}
}
