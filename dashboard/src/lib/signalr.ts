import * as signalR from '@microsoft/signalr';
import type { CallInfo, CallStatistics } from './types';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

class CallsHubConnection {
	private connection: signalR.HubConnection;
	private callbacks: {
		onCallStarted: ((call: CallInfo) => void)[];
		onCallEnded: ((callId: string) => void)[];
		onCallUpdated: ((call: CallInfo) => void)[];
		onStatisticsUpdated: ((stats: CallStatistics) => void)[];
		onInitialState: ((state: { Calls: CallInfo[]; Statistics: CallStatistics }) => void)[];
	} = {
		onCallStarted: [],
		onCallEnded: [],
		onCallUpdated: [],
		onStatisticsUpdated: [],
		onInitialState: []
	};

	constructor() {
		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(`${API_URL}/hubs/calls`)
			.withAutomaticReconnect()
			.configureLogging(signalR.LogLevel.Information)
			.build();

		this.setupHandlers();
	}

	private setupHandlers() {
		this.connection.on('CallStarted', (call: CallInfo) => {
			this.callbacks.onCallStarted.forEach((cb) => cb(call));
		});

		this.connection.on('CallEnded', (callId: string) => {
			this.callbacks.onCallEnded.forEach((cb) => cb(callId));
		});

		this.connection.on('CallUpdated', (call: CallInfo) => {
			this.callbacks.onCallUpdated.forEach((cb) => cb(call));
		});

		this.connection.on('StatisticsUpdated', (stats: CallStatistics) => {
			this.callbacks.onStatisticsUpdated.forEach((cb) => cb(stats));
		});

		this.connection.on(
			'InitialState',
			(state: { Calls: CallInfo[]; Statistics: CallStatistics }) => {
				this.callbacks.onInitialState.forEach((cb) => cb(state));
			}
		);

		this.connection.onclose((error) => {
			console.error('SignalR connection closed:', error);
		});

		this.connection.onreconnecting((error) => {
			console.warn('SignalR reconnecting:', error);
		});

		this.connection.onreconnected((connectionId) => {
			console.log('SignalR reconnected:', connectionId);
		});
	}

	async start() {
		try {
			await this.connection.start();
			console.log('SignalR connected');
		} catch (err) {
			console.error('SignalR connection error:', err);
			setTimeout(() => this.start(), 5000);
		}
	}

	async stop() {
		await this.connection.stop();
	}

	onCallStarted(callback: (call: CallInfo) => void) {
		this.callbacks.onCallStarted.push(callback);
	}

	onCallEnded(callback: (callId: string) => void) {
		this.callbacks.onCallEnded.push(callback);
	}

	onCallUpdated(callback: (call: CallInfo) => void) {
		this.callbacks.onCallUpdated.push(callback);
	}

	onStatisticsUpdated(callback: (stats: CallStatistics) => void) {
		this.callbacks.onStatisticsUpdated.push(callback);
	}

	onInitialState(callback: (state: { Calls: CallInfo[]; Statistics: CallStatistics }) => void) {
		this.callbacks.onInitialState.push(callback);
	}

	get state() {
		return this.connection.state;
	}
}

export const callsHub = new CallsHubConnection();
