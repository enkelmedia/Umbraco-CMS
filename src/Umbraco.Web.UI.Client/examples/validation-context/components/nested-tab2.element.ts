import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, html, LitElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UMB_VALIDATION_CONTEXT, umbBindToValidation, UmbValidationContext, type UmbValidationMessage } from "@umbraco-cms/backoffice/validation";

@customElement('nested-tab2')
export default class NestedTab2Element extends UmbElementMixin(LitElement)
{
	readonly validation = new UmbValidationContext(this);

	@state()
	city = '';

	@state()
	country = ''

	@state()
	messages?: UmbValidationMessage[];

	@state()
	totalErrorCount = 0;

	constructor() {
		super();
		this.validation.setDataPath('$.form.tab2');
		this.validation.autoReport();

		this.consumeContext(UMB_VALIDATION_CONTEXT, (validationContext) => {
			console.log('ctx',validationContext);
			this.observe(
				validationContext?.messages.messages,
				(messages) => {
					this.messages = messages;
				},
				'observeTab2ValidationMessages',
			);
		});

		this.validation.messages.messagesOfPathAndDescendant('$.form.tab2').subscribe((value) => {
			this.totalErrorCount = [...new Set(value.map((x) => x.path))].length;
		});
	}

	async #checkTabValidity(){
		const isValidationContextValid = await this.validation!.validate().then(()=>true,()=>false);
		this.validation.report();
	}

	override render() {
		return html`
			<p>Tab 2 content</p>

			<uui-form>
				<form>
					<div>
						<label>City</label>
						<uui-form-validation-message>
							<uui-input
								type="text"
								.value=${this.city}
								@input=${(e: InputEvent) => (this.city = (e.target as HTMLInputElement).value)}
								${umbBindToValidation(this, '$.form.tab2.city', this.city)}
								required></uui-input>
						</uui-form-validation-message>
					</div>
					<label>Country</label>
					<uui-form-validation-message>
						<uui-input
							type="text"
							.value=${this.country}
							@input=${(e: InputEvent) => (this.country = (e.target as HTMLInputElement).value)}
							${umbBindToValidation(this, '$.form.tab2.country', this.country)}
							required></uui-input>
					</uui-form-validation-message>
				</form>
			</uui-form>
			<uui-button
				look="primary"
        color="default"
				@click=${this.#checkTabValidity}>Check validity</uui-button>
			<hr/>
			<p>Tab errors: ${this.totalErrorCount}</p>
			<pre>${JSON.stringify(this.messages,null,4)}</pre>

			`
	}
}
