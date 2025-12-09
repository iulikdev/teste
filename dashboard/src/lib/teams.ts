import * as microsoftTeams from '@microsoft/teams-js';

export async function initializeTeams(): Promise<boolean> {
	try {
		await microsoftTeams.app.initialize();
		console.log('Teams SDK initialized');

		const context = await microsoftTeams.app.getContext();
		console.log('Teams context:', context);

		// Notify Teams that the app is ready
		microsoftTeams.app.notifySuccess();

		return true;
	} catch (error) {
		console.log('Not running in Teams context:', error);
		return false;
	}
}

export async function getTeamsContext() {
	try {
		return await microsoftTeams.app.getContext();
	} catch {
		return null;
	}
}

export function isInTeams(): boolean {
	return typeof window !== 'undefined' && window.parent !== window;
}
