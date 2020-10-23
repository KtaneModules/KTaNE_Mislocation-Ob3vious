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
	private int groups = 12;
	private bool[] highlighted = new bool[6];
	private bool isanimating;

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
						Mazegen(groups, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", '*');
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

	void Start () 
	{
		Mazegen(groups, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", '*');
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
		Debug.LogFormat("[Mislocation #{0}] {1}", _moduleID, grid.Select(z => z.Select(w => (" " + chars.Substring(0, size) + special.ToString())[w]).Join("")).Join("/").Replace("0", " "));
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

	IEnumerator Strike()
	{
		isanimating = true;
		for (int j = 0; j < 6; j++)
		{
			Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
		}
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.75f);
			}
			yield return new WaitForSeconds(0.1f);
			for (int j = 0; j < 6; j++)
			{
				Buttons[j].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.25f);
			}
			yield return new WaitForSeconds(0.1f);
		}
		isanimating = false;
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} submit' to submit the current position, '!{0} reset' to regenerate the maze. '!{0} uldr' to press those directions.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "submit")
			Buttons[5].OnInteract();
		else if (command == "reset")
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
		while (!solved)
		{
			if (grid[coordinates[1]][coordinates[0]] == groups + 1)
				Buttons[5].OnInteract();
			else
				Buttons[4].OnInteract();
			yield return true;
		}
	}
}
