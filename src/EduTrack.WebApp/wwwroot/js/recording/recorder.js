/**
 * Simple Audio Recorder for MP3 Upload using Web Audio API
 */
export class AudioRecorder {
    constructor() {
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.stream = null;
        this.isRecording = false;
        this.audioContext = null;
        this.mediaStreamSource = null;
        this.scriptProcessor = null;
        this.audioBuffer = [];
    }

    async initialize() {
        try {
            // Get microphone access
            this.stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            // Try to use MP3-compatible MediaRecorder first
            const mimeTypes = [
                'audio/mp4; codecs=mp4a.40.2', // MP4 audio (widely supported)
                'audio/webm; codecs=opus',      // WebM with Opus (good quality)
                'audio/webm',                   // Basic WebM
                'audio/wav'                     // WAV fallback
            ];
            
            let selectedMimeType = null;
            for (const mimeType of mimeTypes) {
                if (MediaRecorder.isTypeSupported(mimeType)) {
                    selectedMimeType = mimeType;
                    break;
                }
            }
            
            if (!selectedMimeType) {
                throw new Error('No supported audio format found');
            }
            
            // Create MediaRecorder with the best available format
            this.mediaRecorder = new MediaRecorder(this.stream, {
                mimeType: selectedMimeType
            });
            
            // Setup event handlers
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.audioChunks.push(event.data);
                }
            };

            this.mediaRecorder.onstop = () => {
                this.isRecording = false;
                this.stream.getTracks().forEach(track => track.stop());
            };

            return true;
        } catch (error) {
            console.error('Failed to initialize recorder:', error);
            throw error;
        }
    }

    async startRecording() {
        if (!this.mediaRecorder) {
            throw new Error('Recorder not initialized');
        }

        if (this.isRecording) {
            throw new Error('Already recording');
        }

        try {
            this.audioChunks = [];
            this.mediaRecorder.start(100);
            this.isRecording = true;
            return true;
        } catch (error) {
            console.error('Failed to start recording:', error);
            throw error;
        }
    }

    async stopRecording() {
        if (!this.isRecording) {
            throw new Error('Not recording');
        }

        return new Promise((resolve, reject) => {
            // Store original onstop handler
            const originalOnStop = this.mediaRecorder.onstop;
            
            this.mediaRecorder.onstop = () => {
                // Restore original handler
                this.mediaRecorder.onstop = originalOnStop;
                
                this.isRecording = false;
                this.stream.getTracks().forEach(track => track.stop());
                
                if (this.audioChunks.length === 0) {
                    reject(new Error('No audio recorded'));
                    return;
                }

                const audioBlob = new Blob(this.audioChunks, { 
                    type: this.mediaRecorder.mimeType 
                });
                
                resolve(audioBlob);
            };

            this.mediaRecorder.stop();
        });
    }

    async uploadAudio(audioBlob) {
        const formData = new FormData();
        
        // Determine the correct file extension based on the blob's MIME type
        let fileName = 'recording.mp3';
        let mimeType = 'audio/mpeg';
        
        if (audioBlob.type.includes('mp4')) {
            fileName = 'recording.m4a';
            mimeType = 'audio/mp4';
        } else if (audioBlob.type.includes('webm')) {
            fileName = 'recording.webm';
            mimeType = 'audio/webm';
        } else if (audioBlob.type.includes('wav')) {
            fileName = 'recording.wav';
            mimeType = 'audio/wav';
        }
        
        formData.append('file', audioBlob, fileName);

        const response = await fetch('/FileUpload/UploadContentFile?type=audio', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const result = await response.json();
        
        if (!result.success) {
            throw new Error(result.message || 'Upload failed');
        }

        return result;
    }

    isCurrentlyRecording() {
        return this.isRecording;
    }

    dispose() {
        if (this.mediaRecorder && this.isRecording) {
            this.mediaRecorder.stop();
        }
        
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
        }
    }
}

// Default instance
export const audioRecorder = new AudioRecorder();