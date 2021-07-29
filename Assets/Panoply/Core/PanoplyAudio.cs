using UnityEngine;
using System;
using System.Collections;

/**
 * The PanoplyAudio class encapsulates static methods for
 * the playback and management of audio.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public enum Fade {In, Out}

	public class PanoplyAudio:MonoBehaviour{
	    
	    public void Start() {
	    
	    }
	    
	    public void Update() {
	    
	    }
	    
	    /**
	     * Create a temporary audio source and use it to play the clip, then destroy it. 
	     * Source: http://forum.unity3d.com/threads/83065-Seamless-Loop-based-Interactive-Adaptive-Music-Player-Sequencer-can-it-be-done
	     */
	    public static AudioSource PlayClip(AudioClip sound,
	                                       Transform tf,
	                                       float pitch,
	                                       float volume,
	                                       int startSample,
	                                       int samplesToGo) {
	        GameObject soundObject = new GameObject(sound.name+UnityEngine.Random.value); 
	        AudioSource soundSource = soundObject.AddComponent<AudioSource>(); 
	        soundSource.clip = sound;
	        soundSource.pitch = pitch;
	        soundSource.volume = volume;
	        soundSource.timeSamples = startSample;
	        soundObject.transform.parent = tf;
	        soundObject.transform.position = tf.position;
	        soundSource.priority = 1;
	        //soundSource.Play(samplesToGo);
	        soundSource.PlayDelayed(samplesToGo / 44100.0f);
	        Destroy(soundObject, sound.length + 1 + (samplesToGo / 44100.0f));
	    	return soundSource;
	    }
	    
	    /**
	     * Create an audio source and use it to play the clip as a loop.
	     */
	    public static AudioSource PlayLoopingClip(AudioClip sound,Transform tf,float volume) {
	        GameObject soundObject = new GameObject(sound.name+UnityEngine.Random.value); 
	        AudioSource soundSource = soundObject.AddComponent<AudioSource>(); 
	        soundSource.clip = sound;
	        soundSource.volume = volume;
	        soundObject.transform.parent = tf;
	        soundObject.transform.position = tf.position;
	        soundSource.priority = 1;
	        soundSource.loop = true;
	        soundSource.Play();
	    	return soundSource;
	}

	    /**
	     * Fades an audio source to a specific level.
	     * Source: http://forum.unity3d.com/threads/26696-How-To-gt-Fade-Out-Music-Before-Loading-Level (modified by Erik Loyer)
	     *
	     * @param audio			The audio source to fade.
	     * @param volume		The destination volume.
	     */
	    public static IEnumerator SetAudioVolume(AudioSource audio,float volume) {
	    	audio.volume = volume;
	    	return null;
	    } 
	    
	    /**
	     * Fades an audio source to a specific level.
	     * Source: http://forum.unity3d.com/threads/26696-How-To-gt-Fade-Out-Music-Before-Loading-Level (modified by Erik Loyer)
	     *
	     * @param audio			The audio source to fade.
	     * @param timer			Duration of the fade in seconds.
	     * @param volume		The destination volume.
	     */
	    public static IEnumerator FadeAudioTo(AudioSource audio,float timer,float volume) {
	        float start = audio.volume;
	        float end = volume;
	        float i = 0.0f;
	        float step = 1.0f/timer;
	        while (i <= 1.0f) {
	            i += step * Time.deltaTime;
	            audio.volume = Mathf.Lerp(start, end, i);
	            yield return null;
	        }
	    } 
	    
	    /**
	     * Fades an audio source in or out.
	     * Source: http://forum.unity3d.com/threads/26696-How-To-gt-Fade-Out-Music-Before-Loading-Level (modified by Erik Loyer)
	     *
	     * @param audio			The audio source to fade.
	     * @param timer			Duration of the fade in seconds.
	     * @param fadeType		Whether to fade in or out.
	     */
	    public static IEnumerator FadeAudio(AudioSource audio,float timer,Fade fadeType) {
	        float start = fadeType == Fade.In? 0.0f : 1.0f;
	        float end = fadeType == Fade.In? 1.0f : 0.0f;
	        float i = 0.0f;
	        float step = 1.0f/timer;
	        while (i <= 1.0f) {
	            i += step * Time.deltaTime;
	            audio.volume = Mathf.Lerp(start, end, i);
	            yield return null;
	        }
	        if (fadeType == Fade.Out) {
	        	audio.Stop();
	        }
	    }   
	}
}