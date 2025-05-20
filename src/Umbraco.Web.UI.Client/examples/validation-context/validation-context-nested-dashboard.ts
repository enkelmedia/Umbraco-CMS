import { html, customElement, css, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UMB_VALIDATION_CONTEXT, UmbValidationContext, type UmbValidationMessage } from '@umbraco-cms/backoffice/validation';
import { validate } from 'uuid';

@customElement('umb-example-validation-context-nested-dashboard')
export class UmbExampleValidationContextNestedDashboardElement extends UmbLitElement {

	readonly validation = new UmbValidationContext(this);

	@state()
  routes: UmbRoute[] = [
		{
			path : 'tab-1',
			component: ()=> import('./components/nested-tab1.element.js')
		},
		{
			path : 'tab-2',
			component: ()=> import('./components/nested-tab2.element.js')
		},
		{
			path: '',
			redirectTo: 'tab-1',
		}
	];

	@state()
	routerRootPath = '';

	@state()
	messages?: UmbValidationMessage[];

	@state()
	totalErrorCount = 0;

	constructor() {
		super();

		this.validation.setDataPath('$.form');

		this.consumeContext(UMB_VALIDATION_CONTEXT, (validationContext) => {
			console.log('ctx root',validationContext);
			this.observe(
				validationContext?.messages.messages,
				(messages) => {
					this.messages = messages;
				},
				'observeRootValidationMessages',
			);
		});

		this.validation.messages.messagesOfPathAndDescendant('$.form').subscribe((value) => {
			this.totalErrorCount = [...new Set(value.map((x) => x.path))].length;
		});
	}

	onRouterInit(event : UmbRouterSlotInitEvent) {
    this.routerRootPath = event.target.absoluteRouterPath ?? "";
  }

	onRouterChange(event : UmbRouterSlotChangeEvent) {
		console.log('trigger validation');
		this.validation.validate();
	}

		async #checkTabValidity(){
		const isValidationContextValid = await this.validation!.validate().then(()=>true,()=>false);
	}

	override render() {
		return html`
			<uui-box>
				<h2>Nested Validation Contexts</h2>
				<nav>
					<a href=${this.routerRootPath + '/tab-1'}>Tab 1</a> <a href=${this.routerRootPath + '/tab-2'}>Tab 2</a>
				</nav>
				<hr/>
				Total errors: ${this.totalErrorCount}
				<pre>${JSON.stringify(this.messages)}</pre>
				<uui-button
					look="primary"
					color="default"
					@click=${this.#checkTabValidity}>Check all validity</uui-button>
				<hr/>
				<umb-router-slot
					.routes=${this.routes}
					@init=${this.onRouterInit}
					@change=${this.onRouterChange}></umb-router-slot>
			</uui-box>
		`;
	}

	static override styles = [
		css`

		`,
	];
}

export default UmbExampleValidationContextNestedDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-validation-context-nested-dashboard': UmbExampleValidationContextNestedDashboardElement;
	}
}
