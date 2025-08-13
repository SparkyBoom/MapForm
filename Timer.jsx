import { useEffect } from "react";

export default function Scheduler() {
  // Step 1: Store your scheduled dates/times
  const schedule = [
    new Date("2025-08-13T15:30:00"),
    new Date("2025-08-14T09:00:00"),
    new Date("2025-08-15T12:00:00"),
  ];

  const runMyFunction = () => {
    console.log("Function executed at:", new Date());
    // Put your actual logic here
  };

  useEffect(() => {
    const interval = setInterval(() => {
      const now = new Date();
      schedule.forEach((date) => {
        // Step 2: Check if current time matches within 1 minute
        if (
          Math.abs(date.getTime() - now.getTime()) < 60 * 1000 // 1 minute tolerance
        ) {
          runMyFunction();
        }
      });
    }, 1000); // Check every second

    return () => clearInterval(interval);
  }, []);

  return <div>Scheduler running…</div>;
}
