/**
 * Simple Audio Recorder for MP3 Upload
 */
export class AudioRecorder {
    constructor() {
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.stream = null;
        this.isRecording = false;
    }

    async initialize() {
        try {
            // Get microphone access
            this.stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            // Create MediaRecorder
            this.mediaRecorder = new MediaRecorder(this.stream);
            
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
        formData.append('file', audioBlob, 'recording.webm');

        const response = await fetch('/FileUpload/api/audio', {
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