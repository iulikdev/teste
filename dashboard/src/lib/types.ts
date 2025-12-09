export interface CallInfo {
	callId: string;
	displayName: string;
	phoneNumber: string;
	agentId: string;
	agentName: string;
	status: CallStatus;
	startTime: string;
	endTime?: string;
	duration: string;
	direction: CallDirection;
}

export enum CallStatus {
	Ringing = 'Ringing',
	InQueue = 'InQueue',
	Connected = 'Connected',
	OnHold = 'OnHold',
	Transferring = 'Transferring',
	Ended = 'Ended'
}

export enum CallDirection {
	Inbound = 'Inbound',
	Outbound = 'Outbound'
}

export interface CallStatistics {
	activeCalls: number;
	callsInQueue: number;
	totalCallsToday: number;
	answeredCalls: number;
	missedCalls: number;
	averageWaitTime: string;
	averageCallDuration: string;
	availableAgents: number;
	busyAgents: number;
	lastUpdated: string;
}

export interface AgentInfo {
	agentId: string;
	displayName: string;
	email: string;
	status: AgentStatus;
	currentCallId?: string;
	callsHandledToday: number;
	totalTalkTime: string;
}

export enum AgentStatus {
	Available = 'Available',
	Busy = 'Busy',
	Away = 'Away',
	Offline = 'Offline'
}
