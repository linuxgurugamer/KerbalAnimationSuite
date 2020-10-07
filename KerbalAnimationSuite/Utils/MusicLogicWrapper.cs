using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public class MusicLogicWrapper
	{
		//delegates
		public delegate void OnFadeEnd(AudioSource source);

		//constructor
		public MusicLogicWrapper()
		{
			musicLogic = MusicLogic.fetch;
		}

		//playlists
		public List<AudioClip> EditorPlaylist
		{
			get{return musicLogic.constructionPlaylist;}
		}
		public List<AudioClip> SpacePlaylist
		{
			get{return musicLogic.spacePlaylist;}
		}
		public List<AudioClip> GetNewPlaylist(params AudioClip[] audio)
		{
			return new List<AudioClip> (audio);
		}

		//running playlist
		private List<AudioClip> _playlist;
		public List<AudioClip> Playlist
		{
			get {if (_playlist == null) return EditorPlaylist; else return _playlist;}
			set {_playlist = value;}
		}

		public bool MusicIsPlaying {get; private set;}

		//music object
		private MusicLogic musicLogic;

		//running coroutines
		private IEnumerator runningPlaylistRoutine;

		//utility methods
		public void StartPlaylist(float fade)
		{
			MusicIsPlaying = true;
			runningPlaylistRoutine = PlaylistRoutine(musicLogic.audio2, Playlist);
			musicLogic.StartCoroutine(runningPlaylistRoutine);
			musicLogic.audio2.volume = 0f;
			musicLogic.StartCoroutine(FadeRoutine(musicLogic.audio2, GameSettings.MUSIC_VOLUME, fade));
		}
		public void StopPlaylist(float fade)
		{
			MusicIsPlaying = false;
			musicLogic.StartCoroutine(FadeRoutine(musicLogic.audio2, 0f, fade, new OnFadeEnd(stopPlaylist)));
		}
		private void stopPlaylist(AudioSource source)
		{
			if (runningPlaylistRoutine != null)
				musicLogic.StopCoroutine(runningPlaylistRoutine);
			source.clip = null;
		}

		//coroutines
		IEnumerator FadeRoutine(AudioSource source, float end, float time, OnFadeEnd onFadeEnd = null)
		{
			if (time <= 0)
			{
				source.volume = end;
			}
			else
			{
				float start = source.volume;
				float currentTime = 0f;
				if (start == end)
				{
					//do nothing
				}
				else
				{
					while (source.volume != end)
					{
						source.volume = Mathf.Lerp(start, end, currentTime);
						currentTime += Time.deltaTime / time;
						yield return null;
					}
					source.volume = end;
					if (onFadeEnd != null)
						onFadeEnd.Invoke(source);
				}
			}
		}
		IEnumerator PlaylistRoutine(AudioSource source, List<AudioClip> playlist)
		{
			int counter = new System.Random().Next(playlist.Count);
			while (true)
			{
				if (counter < 0)
					counter = playlist.Count - 1;
				if (counter >= playlist.Count)
					counter = 0;

				source.clip = playlist [counter];
				source.Play();
				//yield return new WaitForSeconds(source.clip.length + 1f);
				yield return new WaitForSeconds(source.clip.length + 2f);
				counter++;
			}
		}
	}
}

