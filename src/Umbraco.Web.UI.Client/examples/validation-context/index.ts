import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';

const dashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'Demo.Dashboard',
	name: 'Demo Dashboard Validation Context',
	weight: 1000,
	element: () => import('./validation-context-dashboard.js'),
	meta: {
		label: 'Validation Context Demo',
		pathname: 'demo',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: 'Umb.Section.Content',
		},
	],
};

const dashboardNested : ManifestDashboard = {
	type: 'dashboard',
	alias: 'Demo.Dashboard.Nested',
	name: 'Demo Dashboard Validation Context Nested',
	weight: 1000,
	element: () => import('./validation-context-nested-dashboard.js'),
	meta: {
		label: 'Validation Context Demo (Nested)',
		pathname: 'demo-nested',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: 'Umb.Section.Content',
		},
	],
};

export const manifests = [
	dashboard,
	dashboardNested
];
