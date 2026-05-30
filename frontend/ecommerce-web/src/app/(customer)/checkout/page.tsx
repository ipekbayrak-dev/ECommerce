import CheckoutClient from "./checkout-client";

export default function CheckoutPage() {
  const stripeKey = process.env.STRIPE_PUBLISHABLE_KEY ?? "";
  return <CheckoutClient stripeKey={stripeKey} />;
}
