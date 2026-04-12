
class MapaInteractivo {
    constructor(containerId, options = {}) {
        this.containerId = containerId;
        this.map = null;
        this.marker = null;
        this.googleApiKey = options.googleApiKey || "";
        this.defaultCenter = options.center || [-17.7833, -63.1821];
        this.zoom = options.zoom || 13;
        
        this.readOnly = options.readOnly || false;
        
        this.inputs = {
            lat: options.latInputId ? document.getElementById(options.latInputId) : null,
            lng: options.lngInputId ? document.getElementById(options.lngInputId) : null,
            direccion: options.direccionInputId ? document.getElementById(options.direccionInputId) : null
        };

        this.init();
    }

    async init() {
        try {
            if (typeof L === 'undefined') return;

            this.map = L.map(this.containerId).setView(this.defaultCenter, this.zoom);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap'
            }).addTo(this.map);

            const valLat = parseFloat(this.inputs.lat?.value);
            const valLng = parseFloat(this.inputs.lng?.value);
            const startLat = !isNaN(valLat) ? valLat : this.defaultCenter[0];
            const startLng = !isNaN(valLng) ? valLng : this.defaultCenter[1];

            this.marker = L.marker([startLat, startLng], {
                draggable: !this.readOnly,
                icon: L.icon({
                    iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
                    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41]
                })
            }).addTo(this.map);

            this.marker.on('dragend', () => {
                const pos = this.marker.getLatLng();
                this.updateCoords(pos.lat, pos.lng);
                this.reverseGeocode(pos.lat, pos.lng);
            });

            this.map.on('click', (e) => {
                this.marker.setLatLng(e.latlng);
                this.updateCoords(e.latlng.lat, e.latlng.lng);
                this.reverseGeocode(e.latlng.lat, e.latlng.lng);
            });

            if (this.inputs.lat?.value && this.inputs.lng?.value) {
                this.map.setView([startLat, startLng], 16);
            }


            const updateMapFromManualInput = () => {
                const manualLat = parseFloat(this.inputs.lat?.value);
                const manualLng = parseFloat(this.inputs.lng?.value);
                if (!isNaN(manualLat) && !isNaN(manualLng)) {
                    this.map.setView([manualLat, manualLng], 16);
                    this.marker.setLatLng([manualLat, manualLng]);
                    this.reverseGeocode(manualLat, manualLng);
                }
            };

            if (this.inputs.lat && this.inputs.lng) {
                this.inputs.lat.addEventListener('change', updateMapFromManualInput);
                this.inputs.lng.addEventListener('change', updateMapFromManualInput);
                this.inputs.lat.addEventListener('keyup', (e) => { if(e.key === 'Enter') updateMapFromManualInput() });
                this.inputs.lng.addEventListener('keyup', (e) => { if(e.key === 'Enter') updateMapFromManualInput() });
            }

        } catch (error) {
            console.error("Error mapa:", error);
        }
    }

    updateCoords(lat, lng) {
        if (this.inputs.lat) this.inputs.lat.value = lat.toFixed(6);
        if (this.inputs.lng) this.inputs.lng.value = lng.toFixed(6);
    }

    centrarEn(lat, lng, zoom = 12) {
        this.map.flyTo([lat, lng], zoom);
        this.marker.setLatLng([lat, lng]);
        this.updateCoords(lat, lng);
        this.reverseGeocode(lat, lng);
    }


    async reverseGeocode(lat, lng, useOSM = false) {
        let url = "";
        let isGoogle = this.googleApiKey && !useOSM;

        if (isGoogle) {
            url = `https://maps.googleapis.com/maps/api/geocode/json?latlng=${lat},${lng}&key=${this.googleApiKey}`;
        } else {
            url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`;
        }

        try {
            const resp = await fetch(url);
            const data = await resp.json();
            let address = "";

            if (isGoogle) {
                address = data.results?.[0]?.formatted_address || "";
            } else {
                address = data.display_name || "";
            }

            if (address && this.inputs.direccion) {
                this.inputs.direccion.value = address;
            } else if (isGoogle) {

                this.reverseGeocode(lat, lng, true);
            }
        } catch (e) {
            if (isGoogle) this.reverseGeocode(lat, lng, true);
        }
    }

    async buscarDireccion(direccionInput, useOSM = false) {
        const query = (direccionInput || this.inputs.direccion?.value || "").trim();
        if (!query) return;

        let url = "";
        let isGoogle = this.googleApiKey && !useOSM;

        if (isGoogle) {
            url = `https://maps.googleapis.com/maps/api/geocode/json?address=${encodeURIComponent(query)}&key=${this.googleApiKey}`;
        } else {
            url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=1`;
        }

        try {
            const resp = await fetch(url);
            const data = await resp.json();
            let loc = null;

            if (isGoogle && data.results?.[0]) {
                loc = data.results[0].geometry.location;
            } else if (data[0]) {
                loc = { lat: parseFloat(data[0].lat), lng: parseFloat(data[0].lon) };
            }

            if (loc) {
                this.map.flyTo([loc.lat, loc.lng], 16);
                this.marker.setLatLng([loc.lat, loc.lng]);
                this.updateCoords(loc.lat, loc.lng);
            } else if (isGoogle) {

                this.buscarDireccion(query, true);
            }
        } catch (e) {
            if (isGoogle) this.buscarDireccion(query, true);
        }
    }
}
