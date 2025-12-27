import { Outlet, useLocation } from "react-router-dom"
import { cn } from "@/lib/utils"
import { Header } from "@/components/app/Header"
import { Footer } from "@/components/app/Footer"

export default function Layout() {
  const location = useLocation()
  const isDocs = location.pathname.startsWith("/docs")
  const isApi = location.pathname.startsWith("/api")
  const isComponents = location.pathname.startsWith("/components")

  const layoutClasses = cn(
    "min-h-screen flex flex-col transition-[padding] duration-300 ease-in-out",
    (isDocs || isApi) && "lg:pl-72",
    isComponents && "md:pl-60 lg:pl-64"
  )

  return (
    <div className={layoutClasses}>
      {/* Header */}
      <Header/>

      {/* Main Content */}
      <main className="flex-1">
        <Outlet />
      </main>

      {/* Footer */}
      <Footer />
    </div>
  )
}