// YouTube Player API Integration for Coffee Tunes
// This module manages YouTube iframe players and provides play/pause event handling

window.youtubePlayerModule = {
    players: {},
    dotNetHelper: null,
    isYouTubeApiReady: false,

    // Initialize the YouTube IFrame API
    initializeApi: function () {
        if (window.YT && window.YT.Player) {
            this.isYouTubeApiReady = true;
            return Promise.resolve();
        }

        return new Promise((resolve) => {
            // Load the IFrame Player API code asynchronously
            const tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            const firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

            // The API will call this function when it's ready
            window.onYouTubeIframeAPIReady = () => {
                this.isYouTubeApiReady = true;
                resolve();
            };
        });
    },

    // Create a YouTube player for a specific video
    createPlayer: function (elementId, videoId, dotNetHelper, franchiseId, barId) {
        return new Promise(async (resolve, reject) => {
            try {
                // Ensure API is loaded
                if (!this.isYouTubeApiReady) {
                    await this.initializeApi();
                }

                this.dotNetHelper = dotNetHelper;

                // Dispose existing player if any
                if (this.players[elementId]) {
                    this.players[elementId].destroy();
                }

                // Create the player
                // Note: Don't specify height/width here - let CSS handle it via absolute positioning
                this.players[elementId] = new YT.Player(elementId, {
                    videoId: videoId,
                    playerVars: {
                        'playsinline': 1,
                        'rel': 0,
                        'modestbranding': 1
                    },
                    events: {
                        'onReady': (event) => {
                            console.log('YouTube player ready:', elementId);
                            resolve();
                        },
                        'onStateChange': (event) => this.onPlayerStateChange(event, elementId, franchiseId, barId),
                        'onError': (event) => {
                            console.error('YouTube player error:', event.data);
                            reject(new Error('YouTube player error: ' + event.data));
                        }
                    }
                });
            } catch (error) {
                console.error('Error creating YouTube player:', error);
                reject(error);
            }
        });
    },

    // Handle player state changes
    onPlayerStateChange: function (event, elementId, franchiseId, barId) {
        const playerState = event.data;
        console.log('Player state changed:', elementId, 'State:', playerState);

        // YT.PlayerState constants:
        // -1 (unstarted), 0 (ended), 1 (playing), 2 (paused), 3 (buffering), 5 (video cued)
        if (playerState === YT.PlayerState.PLAYING) {
            // Notify Blazor that the video started playing
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnVideoPlayedFromJs', franchiseId, barId)
                    .catch(err => console.error('Error invoking OnVideoPlayedFromJs:', err));
            }
        } else if (playerState === YT.PlayerState.PAUSED) {
            // Notify Blazor that the video was paused
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnVideoPausedFromJs', franchiseId, barId)
                    .catch(err => console.error('Error invoking OnVideoPausedFromJs:', err));
            }
        }
    },

    // Play the video
    playVideo: function (elementId) {
        const player = this.players[elementId];
        if (player && player.playVideo) {
            console.log('Playing video:', elementId);
            player.playVideo();
        } else {
            console.warn('Player not found or not ready:', elementId);
        }
    },

    // Pause the video
    pauseVideo: function (elementId) {
        const player = this.players[elementId];
        if (player && player.pauseVideo) {
            console.log('Pausing video:', elementId);
            player.pauseVideo();
        } else {
            console.warn('Player not found or not ready:', elementId);
        }
    },

    // Dispose of a player
    disposePlayer: function (elementId) {
        const player = this.players[elementId];
        if (player) {
            console.log('Disposing player:', elementId);
            player.destroy();
            delete this.players[elementId];
        }
    }
};
