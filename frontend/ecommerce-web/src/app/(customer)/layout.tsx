import Navbar from "@/components/shared/Navbar";

export default function CustomerLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar />
      <main className="container py-4">{children}</main>
    </>
  );
}
