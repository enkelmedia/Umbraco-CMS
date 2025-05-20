import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, html, LitElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UMB_VALIDATION_CONTEXT, umbBindToValidation, UmbValidationContext, type UmbValidationMessage } from "@umbraco-cms/backoffice/validation";

@customElement('nested-tab1')
export default class NestedTab1Element extends UmbElementMixin(LitElement)
{
	readonly validation = new UmbValidationContext(this);

	@state()
	name = '';

	@state()
	email = '';

	@state()
	messages?: UmbValidationMessage[];

	@state()
	totalErrorCount = 0;

	constructor() {
		super();
		this.validation.setDataPath('$.form.tab1');
		this.validation.autoReport();

		this.consumeContext(UMB_VALIDATION_CONTEXT, (validationContext) => {
			console.log('ctx',validationContext);
			this.observe(
				validationContext?.messages.messages,
				(messages) => {
					this.messages = messages;
				},
				'observeTab1ValidationMessages',
			);
		});

		this.validation.messages.messagesOfPathAndDescendant('$.form.tab1').subscribe((value) => {
			this.totalErrorCount = [...new Set(value.map((x) => x.path))].length;
		});
	}

	async #checkTabValidity(){
		const isValidationContextValid = await this.validation!.validate().then(()=>true,()=>false);
		this.validation.report();
	}

	override render() {
		return html`
			<p>Tab 1 content</p>
			<umb-debug visible dialog></umb-debug>
			<uui-form>
				<form>
					<div>
						<label>Name</label>
						<uui-form-validation-message>
							<uui-input
								type="text"
								.value=${this.name}
								@input=${(e: InputEvent) => (this.name = (e.target as HTMLInputElement).value)}
								${umbBindToValidation(this, '$.form.tab1.name', this.name)}
								required></uui-input>
						</uui-form-validation-message>
					</div>
					<label>E-mail</label>
					<uui-form-validation-message>
						<uui-input
							type="email"
							.value=${this.email}
							@input=${(e: InputEvent) => (this.email = (e.target as HTMLInputElement).value)}
							${umbBindToValidation(this, '$.form.tab1.email', this.email)}
							required></uui-input>
					</uui-form-validation-message>
				</form>
			</uui-form>
			<uui-button
				look="primary"
        color="default"
				@click=${this.#checkTabValidity}>Check validity</uui-button>

			<p>Tab errors: ${this.totalErrorCount}</p>
			<pre>${JSON.stringify(this.messages,null,4)}</pre>
			`
	}
}
