import { ChevronRight } from "lucide-react"
import { Button } from "./Button"
import type React from "react"

interface Props {
  setMobileSidebarOpen: (open: boolean) => void
}

const MobileNavOpenButton: React.FC<Props> = ({ setMobileSidebarOpen }) => {
  return (
    <Button
      className="fixed bottom-6 left-6 z-50 flex h-12 w-12 items-center justify-center rounded-full bg-blue-600 p-0 text-white shadow-lg hover:bg-blue-700 md:hidden"
      onClick={() => setMobileSidebarOpen(true)}
    >
      <ChevronRight className="h-6 w-6" />
    </Button>
  )
}

export default MobileNavOpenButton
